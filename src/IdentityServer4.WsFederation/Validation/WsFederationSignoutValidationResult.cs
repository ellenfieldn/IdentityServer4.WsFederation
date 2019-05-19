// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer4.WsFederation.Validation
{
    public class WsFederationSignoutValidationResult : ValidationResult
    {
        public WsFederationSignoutValidationResult(ValidatedWsFederationSignoutRequest request)
        {
            ValidatedRequest = request;
            IsError = false;
        }

        public WsFederationSignoutValidationResult(ValidatedWsFederationSignoutRequest request, string error, string errorDescription = null)
        {
            ValidatedRequest = request;
            IsError = true;
            Error = error;
            ErrorDescription = errorDescription;
        }

        public ValidatedWsFederationSignoutRequest ValidatedRequest { get; }
    }
}
