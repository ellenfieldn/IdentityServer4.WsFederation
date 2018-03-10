using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation
{
    public class WsFederationMetadataResult : IEndpointResult
    {
        public string Metadata { get; }

        public WsFederationMetadataResult(string metadata)
        {
            Metadata = metadata;
        }

        public Task ExecuteAsync(HttpContext context)
        {
            return context.Response.WriteAsync(Metadata);
        }
    }
}
