using IdentityServer4.Configuration;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation
{
    public class WsFederationSigninResult : IEndpointResult
    {
        public WsFederationSigninResponse Response { get; }

        public WsFederationSigninResult(WsFederationSigninResponse response)
        {
            Response = response;
        }

        private ISystemClock _clock;
        private IMessageStore<ErrorMessage> _errorMessageStore;
        private IdentityServerOptions _options;
        private IUserSession _userSession;

        public async Task ExecuteAsync(HttpContext context)
        {
            _options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            _clock = context.RequestServices.GetRequiredService<ISystemClock>();
            _errorMessageStore = context.RequestServices.GetRequiredService<IMessageStore<ErrorMessage>>();
            _userSession = context.RequestServices.GetRequiredService<IUserSession>();

            if (Response.IsError)
            {
                await ProcessErrorAsync(context);
            }
            else
            {
                await ProcessResponseAsync(context);
            }
        }

        private async Task ProcessResponseAsync(HttpContext context)
        {
            if(!Response.IsError)
            {
                await _userSession.AddClientIdAsync(Response.Request.Client.ClientId);
            }

            var responseHtml = string.Format(formPostHtml, Response.ResponseMessage.IssuerAddress, Response.ResponseMessage.Wresult, Response.ResponseMessage.Wctx);
            var formPost = Response.ResponseMessage.BuildFormPost();
            context.Response.ContentType = "text/html";
            await context.Response.WriteHtmlAsync(formPost);
        }

        private const string formPostHtml = "<form method='post' action='{0}'><input name='wresult' type='hidden' value='{1}' /><input name='wctx' type='hidden' value='{2}' /><noscript><button>Click to continue</button></noscript></form><script>(function(){{document.forms[0].submit();}})();</script>";

        private async Task ProcessErrorAsync(HttpContext context)
        {
            if(Response.Error == "Invalid wreply")
            {
                await RedirectToErrorPageAsync(context);
            }
            else
            {
                await ProcessResponseAsync(context);
            }
        }

        private async Task RedirectToErrorPageAsync(HttpContext context)
        {
            var errorModel = new ErrorMessage
            {
                RequestId = context.TraceIdentifier,
                Error = Response.Error,
                ErrorDescription = Response.ErrorDescription,
            };

            var message = new Message<ErrorMessage>(errorModel, _clock.UtcNow.UtcDateTime);
            var id = await _errorMessageStore.WriteAsync(message);

            var errorUrl = _options.UserInteraction.ErrorUrl;

            //TODO: Double check this string concatenation
            var url = errorUrl + _options.UserInteraction.ErrorIdParameter + id;
        }
    }
}
