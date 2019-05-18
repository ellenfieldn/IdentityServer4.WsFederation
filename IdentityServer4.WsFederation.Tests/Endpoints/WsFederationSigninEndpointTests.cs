using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Services;
using IdentityServer4.WsFederation.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation.Tests.Endpoints
{
    [TestClass]
    public class WsFederationEndpointSigninTests
    {
        private ClaimsPrincipal _defaultUserPrincipal;
        private ILogger<WsFederationEndpoint> _logger = Substitute.For<ILogger<WsFederationEndpoint>>();
        private IdentityServerOptions _options = new IdentityServerOptions();
        private ValidatedWsFederationSigninRequest _validatedRequest;
        private IWsFederationSigninValidator _signinValidator;
        private IWsFederationSignoutValidator _signoutValidator;
        private IWsFederationResponseGenerator _responseGenerator;
        private IUserSession _userSession;

        [TestInitialize]
        public void TestInitialize()
        {
            _responseGenerator = Substitute.For<IWsFederationResponseGenerator>();
            _userSession = Substitute.For<IUserSession>();
            _validatedRequest = Substitute.For<ValidatedWsFederationSigninRequest>();
            _signinValidator = Substitute.For<IWsFederationSigninValidator>();
            _signinValidator.ValidateAsync(default, default).ReturnsForAnyArgs(new WsFederationSigninValidationResult(_validatedRequest));

            var defaultAuthTime = DateTimeOffset.UtcNow - TimeSpan.FromSeconds(500);
            var defaultIdentity = new ClaimsIdentity("authmethod");
            defaultIdentity.AddClaim(new Claim("auth_time", defaultAuthTime.ToUnixTimeSeconds().ToString()));
            _defaultUserPrincipal = new ClaimsPrincipal(defaultIdentity);
        }

        [TestMethod]
        public async Task PostShouldReturnInvalidMethod()
        {
            var endpoint = new WsFederationEndpoint(_logger, _options, _signinValidator, _signoutValidator, _responseGenerator, _userSession);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = HttpMethods.Post;

            var result = await endpoint.ProcessAsync(httpContext) as StatusCodeResult;
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.MethodNotAllowed, result.StatusCode);
        }

        [TestMethod]
        public async Task FailedValidationReturnsErrorResult()
        {
            _signinValidator.ValidateAsync(default, default).ReturnsForAnyArgs(new WsFederationSigninValidationResult(_validatedRequest, "Mock Error", "This is a mock error."));
            var endpoint = new WsFederationEndpoint(_logger, _options, _signinValidator, _signoutValidator, _responseGenerator, _userSession);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = HttpMethods.Get;

            var result = await endpoint.ProcessAsync(httpContext) as WsFederationSigninResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Response.IsError);
            Assert.AreEqual("Mock Error", result.Response.Error);
            Assert.AreEqual("This is a mock error.", result.Response.ErrorDescription);
        }

        [TestMethod]
        public async Task WhenUserIsNotLoggedIn_LoginResultIsReturned()
        {
            _userSession.GetUserAsync().ReturnsForAnyArgs(Task.FromResult(null as ClaimsPrincipal));
            _validatedRequest.RequestMessage = new WsFederationMessage();
            _options.UserInteraction = new UserInteractionOptions();
            _options.UserInteraction.LoginUrl = "http://Login/Url";
            _options.UserInteraction.LoginReturnUrlParameter = "testParam";
            var endpoint = new WsFederationEndpoint(_logger, _options, _signinValidator, _signoutValidator, _responseGenerator, _userSession);
            var httpContext = Substitute.For<HttpContext>();
            httpContext.Request.Method = HttpMethods.Get;
            httpContext.RequestServices.GetService(typeof(IdentityServerOptions)).Returns(_options);

            var result = await endpoint.ProcessAsync(httpContext) as WsFederationLoginPageResult;
            Assert.IsNotNull(result);
            await result.ExecuteAsync(httpContext);

            //Assert that the user was redirected to the login page
            httpContext.Response.Received().RedirectToAbsoluteUrl("http://Login/Url?testParam=%2Fwsfederation");
        }

        [TestMethod]
        public async Task WhenUserIsLoggedIn_SigninResultIsReturned()
        {
            _userSession.GetUserAsync().ReturnsForAnyArgs(Task.FromResult(_defaultUserPrincipal as ClaimsPrincipal));
            var signinResponse = new WsFederationSigninResponse();
            _responseGenerator.GenerateResponseAsync(_validatedRequest).Returns(signinResponse);
            var endpoint = new WsFederationEndpoint(_logger, _options, _signinValidator, _signoutValidator, _responseGenerator, _userSession);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = HttpMethods.Get;

            var result = await endpoint.ProcessAsync(httpContext) as WsFederationSigninResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(signinResponse, result.Response);
        }

        [TestMethod]
        public async Task WhenUserIsLoggedIn_AndAnErrorOccurs_RedirectToErrorPage()
        {
            _userSession.GetUserAsync().ReturnsForAnyArgs(Task.FromResult(_defaultUserPrincipal as ClaimsPrincipal));
            var signinResponse = new WsFederationSigninResponse();
            signinResponse.Error = "Really bad error.";
            _responseGenerator.GenerateResponseAsync(_validatedRequest).Returns(signinResponse);
            var endpoint = new WsFederationEndpoint(_logger, _options, _signinValidator, _signoutValidator, _responseGenerator, _userSession);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = HttpMethods.Get;

            var result = await endpoint.ProcessAsync(httpContext) as WsFederationSigninResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(signinResponse, result.Response);
        }

        [TestMethod]
        public async Task WhenWfreshIs0_LoginResultIsReturned()
        {
            _userSession.GetUserAsync().ReturnsForAnyArgs(Task.FromResult(_defaultUserPrincipal as ClaimsPrincipal));
            _validatedRequest.RequestMessage = new WsFederationMessage();
            _options.UserInteraction = new UserInteractionOptions();
            _options.UserInteraction.LoginUrl = "http://Login/Url";
            _options.UserInteraction.LoginReturnUrlParameter = "testParam";
            var endpoint = new WsFederationEndpoint(_logger, _options, _signinValidator, _signoutValidator, _responseGenerator, _userSession);
            var httpContext = Substitute.For<HttpContext>();
            httpContext.Request.Method = HttpMethods.Get;
            httpContext.Request.QueryString = new QueryString("?wfresh=0");
            httpContext.RequestServices.GetService(typeof(IdentityServerOptions)).Returns(_options);

            var result = await endpoint.ProcessAsync(httpContext) as WsFederationLoginPageResult;
            Assert.IsNotNull(result);
            await result.ExecuteAsync(httpContext);

            httpContext.Response.Received().RedirectToAbsoluteUrl("http://Login/Url?testParam=%2Fwsfederation");
        }

        [TestMethod]
        public async Task WhenWfreshIsShorterThanAuthInterval_LoginResultIsReturned()
        {
            _userSession.GetUserAsync().ReturnsForAnyArgs(Task.FromResult(_defaultUserPrincipal as ClaimsPrincipal));
            _validatedRequest.RequestMessage = new WsFederationMessage();
            _options.UserInteraction = new UserInteractionOptions();
            _options.UserInteraction.LoginUrl = "http://Login/Url";
            _options.UserInteraction.LoginReturnUrlParameter = "testParam";
            var endpoint = new WsFederationEndpoint(_logger, _options, _signinValidator, _signoutValidator, _responseGenerator, _userSession);
            var httpContext = Substitute.For<HttpContext>();
            httpContext.Request.Method = HttpMethods.Get;
            httpContext.Request.QueryString = new QueryString("?wfresh=1");
            httpContext.RequestServices.GetService(typeof(IdentityServerOptions)).Returns(_options);

            var result = await endpoint.ProcessAsync(httpContext) as WsFederationLoginPageResult;
            Assert.IsNotNull(result);
            await result.ExecuteAsync(httpContext);

            httpContext.Response.Received().RedirectToAbsoluteUrl("http://Login/Url?testParam=%2Fwsfederation");
        }

        [TestMethod]
        public async Task WhenWfreshIsLongerThanAuthInterval_SigninResultIsReturned()
        {
            _userSession.GetUserAsync().ReturnsForAnyArgs(Task.FromResult(_defaultUserPrincipal as ClaimsPrincipal));
            var signinResponse = new WsFederationSigninResponse();
            _responseGenerator.GenerateResponseAsync(_validatedRequest).Returns(signinResponse);
            var endpoint = new WsFederationEndpoint(_logger, _options, _signinValidator, _signoutValidator, _responseGenerator, _userSession);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = HttpMethods.Get;
            httpContext.Request.QueryString = new QueryString("?wfresh=1000");

            var result = await endpoint.ProcessAsync(httpContext) as WsFederationSigninResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(signinResponse, result.Response);
        }
    }
}
