using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Hosting;
using IdentityServer4.Services;
using IdentityServer4.WsFederation.Extensions;
using IdentityServer4.WsFederation.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsFederation;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation
{
    public class WsFederationSigninEndpoint : IEndpointHandler
    {
        private readonly IdentityServerOptions _options;
        private readonly IWsFederationRequestValidator _validator;
        private readonly IWsFederationResponseGenerator _responseGenerator;
        private readonly IUserSession _userSession;

        public WsFederationSigninEndpoint(IdentityServerOptions options, IWsFederationRequestValidator validator, IWsFederationResponseGenerator responseGenerator, IUserSession userSession)
        {
            _options = options;
            _validator = validator;
            _responseGenerator = responseGenerator;
            _userSession = userSession;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            var user = await _userSession.GetUserAsync();

            var queryString = context.Request.QueryString;
            var message = WsFederationMessage.FromQueryString(queryString.Value);
            var validationResult = await _validator.ValidateAsync(message, user);

            if (validationResult.IsError)
            {
                return new WsFederationSigninResult(new WsFederationSigninResponse {
                    Request = validationResult.ValidatedRequest,
                    Error = "Error occurred. Helpful, I know."
                });
            }

            //if needed, show login page
            if(user == null)
            {
                return new WsFederationLoginPageResult(validationResult.ValidatedRequest);
            }

            //Otherwise, return result
            var response = await _responseGenerator.GenerateResponseAsync(validationResult.ValidatedRequest);

            return new WsFederationSigninResult(response);
        }
    }
}
