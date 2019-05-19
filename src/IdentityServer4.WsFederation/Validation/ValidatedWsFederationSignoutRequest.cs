// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Validation;
using Microsoft.IdentityModel.Protocols.WsFederation;

namespace IdentityServer4.WsFederation.Validation
{
    public class ValidatedWsFederationSignoutRequest : ValidatedEndSessionRequest
    {
        public WsFederationMessage RequestMessage { get; set; }
    }
}