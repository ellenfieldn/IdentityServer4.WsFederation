using IdentityServer4.Validation;
using Microsoft.IdentityModel.Protocols.WsFederation;

namespace IdentityServer4.WsFederation.Validation
{
    public class ValidatedWsFederationSignoutRequest : ValidatedEndSessionRequest
    {
        public WsFederationMessage RequestMessage { get; set; }
    }
}