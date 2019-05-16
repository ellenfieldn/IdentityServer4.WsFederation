using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using IdentityServer4.WsFederation.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation.Results
{
    public class WsFederationSignoutResult : IEndpointResult
    {
        private readonly string _wreply;
        private IdentityServerOptions _options;
        private readonly IUserSession _userSession;
        private IMessageStore<LogoutMessage> _logoutMessageStore;
        private ISystemClock _clock;

        public WsFederationSignoutResult(string wreply, IUserSession userSession)
        {
            _wreply = wreply;
            _userSession = userSession;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            _options = _options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
            _clock = _clock ?? context.RequestServices.GetRequiredService<ISystemClock>();
            _logoutMessageStore = _logoutMessageStore ?? context.RequestServices.GetRequiredService<IMessageStore<LogoutMessage>>();

            var tempRequest = new ValidatedEndSessionRequest();

            var sessionId = await _userSession.GetSessionIdAsync();
            if(!string.IsNullOrEmpty(sessionId))
            {
                tempRequest.SessionId = sessionId;
            }

            tempRequest.ClientIds = await _userSession.GetClientListAsync();

            if(!string.IsNullOrEmpty(_wreply))
            {
                tempRequest.PostLogOutUri = _wreply;
            }

            var logoutMessage = new LogoutMessage(tempRequest);
            var msg = new Message<LogoutMessage>(logoutMessage, _clock.UtcNow.UtcDateTime);
            var id = await _logoutMessageStore.WriteAsync(msg);

            var url = _options.UserInteraction.LogoutUrl;
            url = url.AddQueryString(_options.UserInteraction.LogoutIdParameter, id);

            context.Response.RedirectToAbsoluteUrl(url);
        }
    }
}

