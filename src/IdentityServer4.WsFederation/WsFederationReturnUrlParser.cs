using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.WsFederation.Extensions;
using IdentityServer4.WsFederation.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation
{
    public class WsFederationReturnUrlParser : IReturnUrlParser
    {
        private readonly IWsFederationRequestValidator _requestValidator;

        public WsFederationReturnUrlParser(IWsFederationRequestValidator requestValidator)
        {
            _requestValidator = requestValidator;
        }

        public bool IsValidReturnUrl(string returnUrl)
        {
            return returnUrl.IsLocalUrl();
        }

        public async Task<AuthorizationRequest> ParseAsync(string returnUrl)
        {
            if(IsValidReturnUrl(returnUrl))
            {
                var request = new AuthorizationRequest()
                {

                };
            }
            return new AuthorizationRequest();
        }
    }
}
