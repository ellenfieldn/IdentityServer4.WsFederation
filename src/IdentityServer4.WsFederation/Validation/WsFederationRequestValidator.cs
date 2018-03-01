using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Stores;
using Microsoft.IdentityModel.Protocols.WsFederation;

namespace IdentityServer4.WsFederation.Validation
{
    public class WsFederationRequestValidator : IWsFederationRequestValidator
    {
        private readonly IClientStore _clients;

        public WsFederationRequestValidator(IClientStore clients)
        {
            _clients = clients;
        }

        public async Task<WsFederationRequestValidationResult> ValidateAsync(WsFederationMessage message, ClaimsPrincipal user)
        {
            var validatedRequest = new ValidatedWsFederationRequest
            {
                RequestMessage = message,
                Subject = user
            };

            var client = await _clients.FindEnabledClientByIdAsync(message.Wtrealm);
            if(client == null)
            {
                return new WsFederationRequestValidationResult(validatedRequest, "No Client");
            }

            if(!client.RedirectUris.Contains(message.Wreply))
            {
                return new WsFederationRequestValidationResult(validatedRequest, "Invalid redirect uri");
            }
            validatedRequest.SetClient(client);

            //if protocol type is wrong, return error
            if(validatedRequest.Client.ProtocolType != IdentityServerConstants.ProtocolTypes.WsFederation)
            {
                return new WsFederationRequestValidationResult(validatedRequest, "Invalid protocol");
            }

            //Do we need to do any more config lookup a la relying party

            //Figure out what action we are performing
            if(string.IsNullOrEmpty(message.Wa))
            {
                return new WsFederationRequestValidationResult(validatedRequest, "Missing wa");
            }

            switch(message.Wa)
            {
                case WsFederationConstants.WsFederationActions.SignIn:
                    return await ValidateSigninAsync(message, validatedRequest);
                default:
                    return new WsFederationRequestValidationResult(validatedRequest, "Unsupported action");
            }
        }

        private async Task<WsFederationRequestValidationResult> ValidateSigninAsync(WsFederationMessage message, ValidatedWsFederationRequest validatedRequest)
        {
            return new WsFederationRequestValidationResult(validatedRequest);
        }
    }
}
