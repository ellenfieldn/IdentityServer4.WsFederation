// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.WsFederation.Extensions;
using IdentityServer4.WsFederation.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation.Results
{
    public class WsFederationSignoutResult : IEndpointResult
    {
        private IdentityServerOptions _options;
        private IMessageStore<LogoutMessage> _logoutMessageStore;
        private ISystemClock _clock;
        private readonly WsFederationSignoutValidationResult _result;

        public WsFederationSignoutResult(WsFederationSignoutValidationResult result)
        {
            _result = result;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            _options = _options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
            _clock = _clock ?? context.RequestServices.GetRequiredService<ISystemClock>();
            _logoutMessageStore = _logoutMessageStore ?? context.RequestServices.GetRequiredService<IMessageStore<LogoutMessage>>();

            var redirectUrl = _options.UserInteraction.LogoutUrl;

            if (!_result.IsError)
            {
                var validatedRequest = _result.ValidatedRequest;
                var logoutMessage = new LogoutMessage(validatedRequest);
                var msg = new Message<LogoutMessage>(logoutMessage, _clock.UtcNow.UtcDateTime);
                var id = await _logoutMessageStore.WriteAsync(msg);
                redirectUrl = redirectUrl.AddQueryString(_options.UserInteraction.LogoutIdParameter, id);
            }

            context.Response.RedirectToAbsoluteUrl(redirectUrl);
        }
    }
}

