using Microsoft.IdentityModel.Protocols.WsFederation;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation.Validation
{
    public interface IWsFederationSignoutValidator
    {
        Task<WsFederationSignoutValidationResult> ValidateAsync(WsFederationMessage message);
    }
}
