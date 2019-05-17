using IdentityServer4.Validation;
using Microsoft.IdentityModel.Protocols.WsFederation;

namespace IdentityServer4.WsFederation.Validation
{
    public class ValidatedWsFederationSigninRequest : ValidatedRequest
    {
        public WsFederationMessage RequestMessage { get; set; }
    }
}
