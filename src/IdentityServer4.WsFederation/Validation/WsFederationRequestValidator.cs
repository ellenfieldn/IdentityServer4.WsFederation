using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsFederation;

namespace IdentityServer4.WsFederation.Validation
{
    public class WsFederationRequestValidator : IWsFederationRequestValidator
    {
        private readonly IClientStore _clients;
        private readonly ILogger _logger;

        public WsFederationRequestValidator(ILogger<WsFederationRequestValidator> logger, IClientStore clients)
        {
            _logger = logger;
            _clients = clients;
        }

        public async Task<WsFederationRequestValidationResult> ValidateAsync(WsFederationMessage message, ClaimsPrincipal user)
        {
            _logger.LogDebug("Start WsFederation signin request validator.");

            var validatedRequest = new ValidatedWsFederationRequest
            {
                RequestMessage = message,
                Subject = user
            };

            if (string.IsNullOrEmpty(message.Wa))
            {
                _logger.LogError("Wa is missing from the request.", validatedRequest);
                return new WsFederationRequestValidationResult(validatedRequest, "Missing wa", "No 'wa' was specified as part of the request.");
            }

            if (string.IsNullOrEmpty(message.Wtrealm))
            {
                _logger.LogError("Wtrealm is missing from the request.", validatedRequest);
                return new WsFederationRequestValidationResult(validatedRequest, "Missing Wtrealm.", "Wtrealm was not passed in as a parameter.");
            }

            if(string.IsNullOrEmpty(message.Wreply))
            {
                _logger.LogError("Wreply is missing from the request.", validatedRequest);
                return new WsFederationRequestValidationResult(validatedRequest, "Missing Wreply.", "Wreply was not passed in as a parameter.");
            }

            var client = await _clients.FindEnabledClientByIdAsync(message.Wtrealm);
            if(client == null)
            {
                _logger.LogError("There is no client configured that matches the wtrealm parameter of the incoming request.", validatedRequest);
                return new WsFederationRequestValidationResult(validatedRequest, "No Client", "There is no client configured that matches the wtrealm parameter of the incoming request.");
            }

            if(!client.RedirectUris.Contains(message.Wreply))
            {
                _logger.LogError("The passed in redirect url is not valid for the given client.", validatedRequest);
                return new WsFederationRequestValidationResult(validatedRequest, "Invalid redirect uri.", "The passed in redirect url is not valid for the given client.");
            }
            validatedRequest.SetClient(client);

            if(validatedRequest.Client.ProtocolType != IdentityServerConstants.ProtocolTypes.WsFederation)
            {
                _logger.LogError("The client identified by the wtrealm does not support WsFederation.", validatedRequest);
                return new WsFederationRequestValidationResult(validatedRequest, "Invalid protocol.", "The client identified by the wtrealm does not support WsFederation.");
            }

            //TODO: Do we need to do any more config lookup a la relying party

            switch (message.Wa)
            {
                case WsFederationConstants.WsFederationActions.SignIn:
                    {
                        _logger.LogTrace("WsFederation signin request validation successful.");
                        return new WsFederationRequestValidationResult(validatedRequest);
                    }
                default:
                    {
                        _logger.LogError("Unsupported action.", validatedRequest);
                        return new WsFederationRequestValidationResult(validatedRequest, "Unsupported action.", $"Only {WsFederationConstants.WsFederationActions.SignIn} is supported at this time.");
                    }
            }
        }
    }
}
