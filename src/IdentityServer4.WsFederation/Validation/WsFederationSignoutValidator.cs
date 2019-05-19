using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsFederation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation.Validation
{
    public class WsFederationSignoutValidator : IWsFederationSignoutValidator
    {
        private readonly IClientStore _clients;
        private readonly ILogger _logger;
        private readonly IUserSession _userSession;

        public WsFederationSignoutValidator(ILogger<WsFederationSigninValidator> logger, IUserSession userSession, IClientStore clients)
        {
            _logger = logger;
            _clients = clients;
            _userSession = userSession;
        }

        public async Task<WsFederationSignoutValidationResult> ValidateAsync(WsFederationMessage message)
        {
            _logger.LogDebug("Start WsFederation signout request validator.");

            var validatedRequest = new ValidatedWsFederationSignoutRequest
            {
                RequestMessage = message
            };

            validatedRequest.SessionId = await _userSession.GetSessionIdAsync();
            validatedRequest.ClientIds = await _userSession.GetClientListAsync();

            IEnumerable<string> postLogoutRedirectUris;
            //Wtrealm is not really part of the standard for signout, but it appears that some implementations include it.
            //If the wtrealm is specified, respect it. Otherwise, search all of the user's known clients for the wreply.
            if (!string.IsNullOrEmpty(message.Wtrealm))
            {
                _logger.LogDebug("Wtrealm was specified. Using PostLogoutRedirectUris for only that client.", validatedRequest);
                var client = await _clients.FindEnabledClientByIdAsync(message.Wtrealm);
                if (client == null)
                {
                    _logger.LogError("There is no client configured that matches the wtrealm parameter of the incoming request.", validatedRequest);
                    return new WsFederationSignoutValidationResult(validatedRequest, "No Client.", "There is no client configured that matches the wtrealm parameter of the incoming request.");
                }
                postLogoutRedirectUris = client.PostLogoutRedirectUris;
            }
            else
            {
                _logger.LogDebug("Wtrealm was not specified. Using PostLogoutRedirectUris for all clients that the user has authenticated with.", validatedRequest);
                var tasks = validatedRequest.ClientIds.Select(async c => await _clients.FindEnabledClientByIdAsync(c)).ToList();
                var clients = await Task.WhenAll(tasks);
                postLogoutRedirectUris = clients.SelectMany(c => c.PostLogoutRedirectUris).Where(uri => !string.IsNullOrEmpty(uri));
            }

            //This behavior might be odd. If the user is authenticated with multiple clients, it's probably bad to redirect them to a random client....
            if (string.IsNullOrEmpty(message.Wreply))
            {
                _logger.LogInformation("Wreply is missing from the request. Using the defualt wreply.", validatedRequest);
                validatedRequest.PostLogOutUri = postLogoutRedirectUris.FirstOrDefault();
            }
            else if (postLogoutRedirectUris.Contains(message.Wreply))
            {
                validatedRequest.PostLogOutUri = message.Wreply;
            }
            else
            {
                _logger.LogError("The passed in redirect url is not valid for the given client.", validatedRequest);
                return new WsFederationSignoutValidationResult(validatedRequest, "Invalid redirect uri.", "The passed in redirect url is not valid for the given client.");
            }
            _logger.LogTrace("WsFederation signout request validation successful.");
            return new WsFederationSignoutValidationResult(validatedRequest);
        }
    }
}
