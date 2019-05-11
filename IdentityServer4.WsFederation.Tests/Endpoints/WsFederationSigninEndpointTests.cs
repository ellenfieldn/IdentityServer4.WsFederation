using IdentityServer4.Configuration;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Services;
using IdentityServer4.WsFederation.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation.Tests.Endpoints
{
    [TestClass]
    public class WsFederationSigninEndpointTests
    {
        private ILogger<WsFederationSigninEndpoint> _logger = Substitute.For<ILogger<WsFederationSigninEndpoint>>();
        private IdentityServerOptions _options = new IdentityServerOptions();
        private ValidatedWsFederationRequest _validatedRequest;
        private IWsFederationRequestValidator _validator;
        private IWsFederationResponseGenerator _responseGenerator;
        private IUserSession _userSession;

        [TestInitialize]
        public void TestInitialize()
        {
            _responseGenerator = Substitute.For<IWsFederationResponseGenerator>();
            _userSession = Substitute.For<IUserSession>();
            _validatedRequest = Substitute.For<ValidatedWsFederationRequest>();
            _validator = Substitute.For<IWsFederationRequestValidator>();
            _validator.ValidateAsync(default, default).ReturnsForAnyArgs(new WsFederationRequestValidationResult(_validatedRequest));
        }

        [TestMethod]
        public async Task PostShouldReturnInvalidMethod()
        {
            var endpoint = new WsFederationSigninEndpoint(_logger, _options, _validator, _responseGenerator, _userSession);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = HttpMethods.Post;

            var result = await endpoint.ProcessAsync(httpContext) as StatusCodeResult;
            Assert.IsNotNull(result);
            Assert.AreEqual((int)HttpStatusCode.MethodNotAllowed, result.StatusCode);
        }

        [TestMethod]
        public async Task FailedValidationReturnsErrorResult()
        {
            _validator.ValidateAsync(default, default).ReturnsForAnyArgs(new WsFederationRequestValidationResult(_validatedRequest, "Mock Error", "This is a mock error."));
            var endpoint = new WsFederationSigninEndpoint(_logger, _options, _validator, _responseGenerator, _userSession);
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
            var endpoint = new WsFederationSigninEndpoint(_logger, _options, _validator, _responseGenerator, _userSession);
            var httpContext = Substitute.For<HttpContext>();
            httpContext.Request.Method = HttpMethods.Get;
            httpContext.RequestServices.GetService(typeof(IdentityServerOptions)).Returns(_options);

            var result = await endpoint.ProcessAsync(httpContext) as WsFederationLoginPageResult;
            Assert.IsNotNull(result);
            await result.ExecuteAsync(httpContext);

            //Assert that the user was redirected to the login page
            httpContext.Response.Received().RedirectToAbsoluteUrl("http://Login/Url?testParam=%2Fwsfederation%2Fsignin");
        }

        [TestMethod]
        public async Task WhenUserIsLoggedIn_SigninResultIsReturned()
        {
            var signinResponse = new WsFederationSigninResponse();
            _responseGenerator.GenerateResponseAsync(_validatedRequest).Returns(signinResponse);
            var endpoint = new WsFederationSigninEndpoint(_logger, _options, _validator, _responseGenerator, _userSession);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = HttpMethods.Get;

            var result = await endpoint.ProcessAsync(httpContext) as WsFederationSigninResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(signinResponse, result.Response);
        }

        [TestMethod]
        public async Task WhenUserIsLoggedIn_AndAnErrorOccurs_RedirectToErrorPage()
        {
            var signinResponse = new WsFederationSigninResponse();
            signinResponse.Error = "Really bad error.";
            _responseGenerator.GenerateResponseAsync(_validatedRequest).Returns(signinResponse);
            var endpoint = new WsFederationSigninEndpoint(_logger, _options, _validator, _responseGenerator, _userSession);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = HttpMethods.Get;

            var result = await endpoint.ProcessAsync(httpContext) as WsFederationSigninResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(signinResponse, result.Response);
        }
    }
}
