using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsFederation;
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

            //Wtrealm is not really part of the standard for signout, but it appears that some implementations include it
            if (!string.IsNullOrEmpty(message.Wtrealm))
            {
                var client = await _clients.FindEnabledClientByIdAsync(message.Wtrealm);
                if (client != null)
                {
                    if (client.PostLogoutRedirectUris.Contains(message.Wreply))
                    {
                        validatedRequest.PostLogOutUri = message.Wreply;
                    }
                    else
                    {
                        _logger.LogError("The passed in redirect url is not valid for the given client.", validatedRequest);
                        return new WsFederationSignoutValidationResult(validatedRequest, "Invalid redirect uri.", "The passed in redirect url is not valid for the given client.");
                    }
                }
            }

            var clients = (await Task.WhenAll(validatedRequest.ClientIds.Select(async c => await _clients.FindEnabledClientByIdAsync(c))))
                .Where(c => !string.IsNullOrEmpty(c.PostLogoutRedirectUris?.FirstOrDefault()));

            if (string.IsNullOrEmpty(message.Wreply))
            {
                _logger.LogInformation("Wreply is missing from the request. Using the defualt wreply.", validatedRequest);
                validatedRequest.PostLogOutUri = clients.FirstOrDefault().PostLogoutRedirectUris.FirstOrDefault();
                return new WsFederationSignoutValidationResult(validatedRequest);
            }
            else
            {
                if (!clients.Any(c => c.PostLogoutRedirectUris.Contains(message.Wreply)))
                {
                    _logger.LogError("The passed in redirect url is not valid for the given client.", validatedRequest);
                    return new WsFederationSignoutValidationResult(validatedRequest, "Invalid redirect uri.", "The passed in redirect url is not valid for the given client.");
                }
                validatedRequest.PostLogOutUri = message.Wreply;
                return new WsFederationSignoutValidationResult(validatedRequest);
            }
        }
    }
}
