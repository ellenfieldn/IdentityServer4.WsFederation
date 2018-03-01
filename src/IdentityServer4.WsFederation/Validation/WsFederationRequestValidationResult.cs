using IdentityServer4.Validation;

namespace IdentityServer4.WsFederation.Validation
{
    public class WsFederationRequestValidationResult : ValidationResult
    {
        public WsFederationRequestValidationResult(ValidatedWsFederationRequest request)
        {
            ValidatedRequest = request;
            IsError = false;
        }

        public WsFederationRequestValidationResult(ValidatedWsFederationRequest request, string error, string errorDescription = null)
        {
            ValidatedRequest = request;
            IsError = true;
            Error = error;
            ErrorDescription = errorDescription;
        }

        public ValidatedWsFederationRequest ValidatedRequest { get; }
    }
}