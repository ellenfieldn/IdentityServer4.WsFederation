using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.WsFederation.Extensions;
using IdentityServer4.WsFederation.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation
{
    public class WsFederationLoginPageResult : IEndpointResult
    {
        private readonly ValidatedWsFederationRequest _request;
        private IdentityServerOptions _options;

        public WsFederationLoginPageResult(ValidatedWsFederationRequest request)
        {
            _request = request;
        }

        public Task ExecuteAsync(HttpContext context)
        {
            _options = _options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();

            var returnUrl = context.GetIdentityServerBasePath() + "/wsfederation/signin"; //TODO: This probably shouldn't be a hard-coded string.
            returnUrl = returnUrl + _request.RequestMessage.BuildRedirectUrl();

            var loginUrl = _options.UserInteraction.LoginUrl;
            var url = loginUrl.AddQueryString(_options.UserInteraction.LoginReturnUrlParameter, returnUrl); 
            context.Response.RedirectToAbsoluteUrl(url);

            return Task.CompletedTask;
        }
    }
}
