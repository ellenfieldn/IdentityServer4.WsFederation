// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using System.Collections.Generic;
using static IdentityServer4.IdentityServerConstants;

namespace IdentityServer4.WsFederation.Server.Config
{
    public static class Clients
    {
        public static List<Client> TestClients = new List<Client>
        {
            new Client
            {
                ClientId = "urn:idsrv4:wsfed:sample",
                ProtocolType = ProtocolTypes.WsFederation,
                PostLogoutRedirectUris = { "http://localhost:51214/", "http://testredirect/" },
                RedirectUris = { "http://localhost:51214/signin-wsfed" },
            },
            new Client
            {
                ClientId = "urn:idsrv4:wsfed:noredirect",
                ProtocolType = ProtocolTypes.WsFederation,
                RedirectUris = { "http://localhost:51214/signin-wsfed" },
            }
        };
    }
}
