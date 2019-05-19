// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Validation;

namespace IdentityServer4.WsFederation.Validation
{
    public class WsFederationSigninValidationResult : ValidationResult
    {
        public WsFederationSigninValidationResult(ValidatedWsFederationSigninRequest request)
        {
            ValidatedRequest = request;
            IsError = false;
        }

        public WsFederationSigninValidationResult(ValidatedWsFederationSigninRequest request, string error, string errorDescription = null)
        {
            ValidatedRequest = request;
            IsError = true;
            Error = error;
            ErrorDescription = errorDescription;
        }

        public ValidatedWsFederationSigninRequest ValidatedRequest { get; }
    }
}