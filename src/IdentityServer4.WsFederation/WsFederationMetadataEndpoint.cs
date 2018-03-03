using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation
{
    public class WsFederationMetadataEndpoint : IEndpointHandler
    {
        private readonly IdentityServerOptions _options;
        private readonly WsFederationMetadataGenerator _metadataGenerator;

        public WsFederationMetadataEndpoint(IdentityServerOptions options, WsFederationMetadataGenerator metadataGenerator)
        {
            _options = options;
            _metadataGenerator = metadataGenerator;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            if(HttpMethods.IsGet(context.Request.Method))
            {
                var metadata = await _metadataGenerator.GetMetadata(context);
                return new WsFederationMetadataResult(metadata);
            }
            else
            {
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }
        }
    }
}
