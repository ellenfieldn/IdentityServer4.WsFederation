// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityServer4.Services;
using IdentityServer4.WsFederation.Results;
using IdentityServer4.WsFederation.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsFederation;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation
{
    public class WsFederationEndpoint : IEndpointHandler
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly IWsFederationSigninValidator _signinValidator;
        private readonly IWsFederationSignoutValidator _signoutValidator;
        private readonly IWsFederationResponseGenerator _responseGenerator;
        private readonly IUserSession _userSession;

        public WsFederationEndpoint(ILogger<WsFederationEndpoint> logger, IdentityServerOptions options, IWsFederationSigninValidator signinValidator, IWsFederationSignoutValidator signoutValidator, IWsFederationResponseGenerator responseGenerator, IUserSession userSession)
        {
            _logger = logger;
            _options = options;
            _signinValidator = signinValidator;
            _signoutValidator = signoutValidator;
            _responseGenerator = responseGenerator;
            _userSession = userSession;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogDebug("Processing WsFederation request.");

            if (!HttpMethods.IsGet(context.Request.Method))
            {
                _logger.LogWarning($"WsFederation endpoint only supports GET requests. Current method is {context.Request.Method}");
                return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }

            var queryString = context.Request.QueryString;
            _logger.LogDebug($"Proccessing WsFederation signin request with QueryString: {queryString}.");

            var message = WsFederationMessage.FromQueryString(queryString.Value);

            var user = await _userSession.GetUserAsync();

            if (message.IsSignOutMessage)
            {
                var signoutValidationResult = await _signoutValidator.ValidateAsync(message);
                return new WsFederationSignoutResult(signoutValidationResult);
            }
            else //Sign in validator also handles errors for unsupported wa
            {
                var validationResult = await _signinValidator.ValidateAsync(message, user);

                if (validationResult.IsError)
                {
                    _logger.LogError("WsFederation Signin request validation failed.");
                    return new WsFederationSigninResult(new WsFederationSigninResponse
                    {
                        Request = validationResult.ValidatedRequest,
                        Error = validationResult.Error,
                        ErrorDescription = validationResult.ErrorDescription
                    });
                }

                //if needed, show login page
                if (IsLoginRequired(user, message))
                {
                    return new WsFederationLoginPageResult(validationResult.ValidatedRequest);
                }

                //Otherwise, return result
                var response = await _responseGenerator.GenerateResponseAsync(validationResult.ValidatedRequest);

                _logger.LogTrace("End get WsFederation signin request.");
                return new WsFederationSigninResult(response);
            }
        }

        private bool IsLoginRequired(ClaimsPrincipal user, WsFederationMessage message)
        {
            if(user == null || !user.Identity.IsAuthenticated)
            {
                _logger.LogInformation("User is null. Showing login page.");
                return true;
            }

            //If the request contains a wfresh parameter, determine if the session is fresh enough to satisfy the wfresh
            if (long.TryParse(message.Wfresh, out long wfresh))
            {
                var authAge = DateTime.UtcNow - user.GetAuthenticationTime();
                if (authAge.TotalSeconds >= wfresh)
                {
                    _logger.LogInformation("User session does not meet freshness requirement. Forcing re-authentication.");
                    return true;
                }
            }
            return false;
        }
    }
}
