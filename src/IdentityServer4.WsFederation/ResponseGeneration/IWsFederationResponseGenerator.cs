using System.Threading.Tasks;
using IdentityServer4.WsFederation.Validation;

namespace IdentityServer4.WsFederation
{
    public interface IWsFederationResponseGenerator
    {
        Task<WsFederationSigninResponse> GenerateResponseAsync(ValidatedWsFederationRequest validatedRequest);
    }
}