// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

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
