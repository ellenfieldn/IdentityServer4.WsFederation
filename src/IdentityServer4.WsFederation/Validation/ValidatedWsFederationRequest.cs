using IdentityServer4.Validation;
using Microsoft.IdentityModel.Protocols.WsFederation;

namespace IdentityServer4.WsFederation.Validation
{
    public class ValidatedWsFederationRequest : ValidatedRequest
    {
        public WsFederationMessage RequestMessage { get; set; }
    }
}
