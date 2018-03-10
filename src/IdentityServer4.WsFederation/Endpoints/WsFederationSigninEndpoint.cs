using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.Services;
using IdentityServer4.WsFederation.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsFederation;
using System.Net;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation
{
    public class WsFederationSigninEndpoint : IEndpointHandler
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly IWsFederationRequestValidator _validator;
        private readonly IWsFederationResponseGenerator _responseGenerator;
        private readonly IUserSession _userSession;

        public WsFederationSigninEndpoint(ILogger<WsFederationSigninEndpoint> logger, IdentityServerOptions options, IWsFederationRequestValidator validator, IWsFederationResponseGenerator responseGenerator, IUserSession userSession)
        {
            _logger = logger;
            _options = options;
            _validator = validator;
            _responseGenerator = responseGenerator;
            _userSession = userSession;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogDebug("Processing WsFederation signin request.");

            if (!HttpMethods.IsGet(context.Request.Method))
            {
                _logger.LogWarning($"WsFederation signin endpoint only supports GET requests. Current method is {context.Request.Method}");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            var user = await _userSession.GetUserAsync();

            var queryString = context.Request.QueryString;
            _logger.LogDebug($"Proccessing WsFederation signin request with QueryString: {queryString}.");

            var message = WsFederationMessage.FromQueryString(queryString.Value);
            var validationResult = await _validator.ValidateAsync(message, user);

            if (validationResult.IsError)
            {
                _logger.LogError("WsFederation Signin request validation failed.");
                return new WsFederationSigninResult(new WsFederationSigninResponse {
                    Request = validationResult.ValidatedRequest,
                    Error = validationResult.Error,
                    ErrorDescription = validationResult.ErrorDescription
                });
            }

            //if needed, show login page
            if(user == null)
            {
                _logger.LogInformation("User is null. Showing login page.");
                return new WsFederationLoginPageResult(validationResult.ValidatedRequest);
            }

            //Otherwise, return result
            var response = await _responseGenerator.GenerateResponseAsync(validationResult.ValidatedRequest);

            _logger.LogTrace("End get WsFederation signin request.");
            return new WsFederationSigninResult(response);
        }
    }
}
