using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation
{
    public class WsFederationMetadataEndpoint : IEndpointHandler
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly WsFederationMetadataGenerator _metadataGenerator;

        public WsFederationMetadataEndpoint(ILogger<WsFederationMetadataEndpoint> logger, IdentityServerOptions options, WsFederationMetadataGenerator metadataGenerator)
        {
            _logger = logger;
            _options = options;
            _metadataGenerator = metadataGenerator;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogDebug("Processing WsFederation metadata request.");

            if(!HttpMethods.IsGet(context.Request.Method))
            {
                _logger.LogWarning($"WsFederation metadata endpoint only supports GET requests. Current method is {context.Request.Method}");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            if (!_options.Endpoints.EnableDiscoveryEndpoint)
            {
                _logger.LogInformation("WsFederation metadata endpoint is disabled. 404.");
                return new StatusCodeResult(HttpStatusCode.NotFound);
            }

            _logger.LogTrace($"Calling into WsFederation metadata response generator: {_metadataGenerator.GetType().FullName}");
            var metadata = await _metadataGenerator.GetMetadata(context);

            _logger.LogTrace($"End get WsFederation metadata request. Result: {metadata}");
            return new WsFederationMetadataResult(metadata);
        }
    }
}
