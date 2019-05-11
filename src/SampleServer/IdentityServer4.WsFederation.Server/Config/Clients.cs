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
                RedirectUris = { "http://localhost:51213/signin-wsfed" },
            }
        };
    }
}
