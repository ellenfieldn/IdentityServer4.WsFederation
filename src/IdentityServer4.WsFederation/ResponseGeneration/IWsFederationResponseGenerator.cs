// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityServer4.WsFederation.Validation;

namespace IdentityServer4.WsFederation
{
    public interface IWsFederationResponseGenerator
    {
        Task<WsFederationSigninResponse> GenerateResponseAsync(ValidatedWsFederationSigninRequest validatedRequest);
    }
}