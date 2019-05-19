// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.WsFederation.Validation;
using Microsoft.IdentityModel.Protocols.WsFederation;

namespace IdentityServer4.WsFederation
{
    public class WsFederationSigninResponse
    {
        public ValidatedWsFederationSigninRequest Request { get; set; }
        public WsFederationMessage ResponseMessage { get; set; }

        public string Error { get; set; }
        public string ErrorDescription { get; set; }
        public bool IsError => !string.IsNullOrEmpty(Error);
    }
}
