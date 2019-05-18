using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4.WsFederation.Server;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation.Tests.Functional
{
    [TestClass]
    public class SigninTests
    {
        private readonly  HttpClient _client;
        public ClaimsPrincipal _fakePrincipal;

        public SigninTests()
        {
            var factory = new WebApplicationFactory<Program>();
            _client = factory.CreateClient();

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, "TestUser"));
            identity.AddClaim(new Claim(JwtClaimTypes.Subject, "subject"));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "TestUserId"));
            _fakePrincipal = new ClaimsPrincipal(identity);
        }

        public HttpClient GetClient(Action<IServiceCollection> serviceAction) =>
             new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder => builder.ConfigureTestServices(serviceAction))
                .CreateClient(new WebApplicationFactoryClientOptions
                {
                    HandleCookies = true,
                    AllowAutoRedirect = false
                });

        private class FakeUserFilter : IAsyncActionFilter
        {
            private ClaimsPrincipal _fakePrincipal;

            public FakeUserFilter(ClaimsPrincipal fakePrincipal)
            {
                _fakePrincipal = fakePrincipal;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                context.HttpContext.User = _fakePrincipal;
                await next();
            }
        }

        [TestMethod]
        public async Task SigninWithNoContextRedirectsToLogin()
        {
            var response = await _client.GetAsync("/wsfederation?wa=wsignin1.0&wtrealm=urn:idsrv4:wsfed:sample");
            var content = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("/Account/Login", response.RequestMessage.RequestUri.LocalPath);
            StringAssert.Contains(content, "<h1>Login</h1>");
        }

        [TestMethod]
        public async Task SigninWithContext_ReturnsSelfPostRstr()
        {
            var mockSession = Substitute.For<IUserSession>();
            mockSession.GetUserAsync().Returns(_fakePrincipal);
            var client = GetClient(services =>
            {
                services.AddMvcCore(o => o.Filters.Add(new FakeUserFilter(_fakePrincipal)));
                services.AddSingleton(mockSession);
            });

            var response = await client.GetAsync("/wsfederation?wa=wsignin1.0&wtrealm=urn:idsrv4:wsfed:sample");
            var content = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("/wsfederation", response.RequestMessage.RequestUri.LocalPath);
            StringAssert.Contains(content, "RequestSecurityTokenResponse");
        }
    }
}
