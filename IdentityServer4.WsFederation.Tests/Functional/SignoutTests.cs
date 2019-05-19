using HtmlAgilityPack;
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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation.Tests.Functional
{
    [TestClass]
    public class SignoutTests
    {
        private readonly HttpClient _client;
        public ClaimsPrincipal _fakePrincipal;

        public SignoutTests()
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

        private class FakeUserFilter : IAsyncAuthorizationFilter
        {
            private ClaimsPrincipal _fakePrincipal;
            public FakeUserFilter(ClaimsPrincipal fakePrincipal) => _fakePrincipal = fakePrincipal;
            public async Task OnAuthorizationAsync(AuthorizationFilterContext context) => context.HttpContext.User = _fakePrincipal;
        }

        public HttpClient GetClientWithFakeUserContext()
        {
            var mockSession = Substitute.For<IUserSession>();
            mockSession.GetUserAsync().Returns(_fakePrincipal);
            var client = GetClient(services =>
            {
                services.AddMvc(o => o.Filters.Add(new FakeUserFilter(_fakePrincipal)));
                services.AddSingleton(mockSession);
            });
            return client;
        }

        [TestMethod]
        public async Task SignoutWithNoContext_Displays_LogoutPage()
        {
            var response = await _client.GetAsync("/wsfederation?wa=wsignout1.0&wtrealm=urn:idsrv4:wsfed:sample");
            var content = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("/Account/Logout", response.RequestMessage.RequestUri.LocalPath);
            StringAssert.Contains(content, "You are now logged out");
        }

        [TestMethod]
        public async Task SignoutWithContext_Displays_ConfirmationPage()
        {
            var client = GetClientWithFakeUserContext();
            var response = await client.GetAsync("/wsfederation?wa=wsignout1.0&wtrealm=urn:idsrv4:wsfed:sample&wreply=http://testredirect/");
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);

            response = await client.GetAsync(response.Headers.Location);
            var content = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("/Account/Logout", response.RequestMessage.RequestUri.LocalPath);
            StringAssert.Contains(content, "Would you like to logout of IdentityServer?");
        }

        [TestMethod]
        public async Task SignoutWithContext_AndConfirmation_DoesNotRedirectWithNoWreply()
        {
            var client = GetClientWithFakeUserContext();
            var response = await client.GetAsync("/wsfederation?wa=wsignout1.0&wtrealm=urn:idsrv4:wsfed:noredirect");
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);

            response = await client.GetAsync(response.Headers.Location);
            var content = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("/Account/Logout", response.RequestMessage.RequestUri.LocalPath);
            StringAssert.Contains(content, "Would you like to logout of IdentityServer?");

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var antiforgeryToken = doc.DocumentNode.SelectSingleNode("//input[@name='__RequestVerificationToken']").Attributes["value"].Value;
            var logoutId = doc.DocumentNode.SelectSingleNode("//input[@name='logoutId']").Attributes["value"].Value;

            var formPost = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", antiforgeryToken),
                new KeyValuePair<string, string>("logoutId", logoutId),
            });

            var postResponse = await client.PostAsync("/Account/Logout", formPost);
            var postContent = await postResponse.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, postResponse.StatusCode);
            Assert.IsFalse(postContent.Contains("PostLogoutRedirectUri"), "Should not have contained a redirect to the wreply.");
        }

        [TestMethod]
        public async Task SignoutWithContext_AndConfirmation_DoesNotRedirectWithInvalidWreply()
        {
            var client = GetClientWithFakeUserContext();
            var response = await client.GetAsync("/wsfederation?wa=wsignout1.0&wtrealm=urn:idsrv4:wsfed:noredirect&wreply=http://testredirect/");
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);

            response = await client.GetAsync(response.Headers.Location);
            var content = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("/Account/Logout", response.RequestMessage.RequestUri.LocalPath);
            StringAssert.Contains(content, "Would you like to logout of IdentityServer?");

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var antiforgeryToken = doc.DocumentNode.SelectSingleNode("//input[@name='__RequestVerificationToken']").Attributes["value"].Value;
            //No Logout ID because mismatch wreply causes an error

            var formPost = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", antiforgeryToken),
                new KeyValuePair<string, string>("logoutId", ""),
            });

            var postResponse = await client.PostAsync("/Account/Logout", formPost);
            var postContent = await postResponse.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, postResponse.StatusCode);
            Assert.IsFalse(postContent.Contains("<a class=\"PostLogoutRedirectUri\" href=\"http://testredirect/\">"), "Should not have contained a redirect to the wreply.");
        }

        [TestMethod]
        public async Task SignoutWithContext_AndConfirmation_RedirectsWithMatchingWreply()
        {
            var client = GetClientWithFakeUserContext();
            var response = await client.GetAsync("/wsfederation?wa=wsignout1.0&wtrealm=urn:idsrv4:wsfed:sample&wreply=http://testredirect/");
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);

            response = await client.GetAsync(response.Headers.Location);
            var content = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("/Account/Logout", response.RequestMessage.RequestUri.LocalPath);
            StringAssert.Contains(content, "Would you like to logout of IdentityServer?");

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var antiforgeryToken = doc.DocumentNode.SelectSingleNode("//input[@name='__RequestVerificationToken']").Attributes["value"].Value;
            var logoutId = doc.DocumentNode.SelectSingleNode("//input[@name='logoutId']").Attributes["value"].Value;

            var formPost = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", antiforgeryToken),
                new KeyValuePair<string, string>("logoutId", logoutId),
            });

            var postResponse = await client.PostAsync("/Account/Logout", formPost);
            var postContent = await postResponse.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, postResponse.StatusCode);
            StringAssert.Contains(postContent, "<a class=\"PostLogoutRedirectUri\" href=\"http://testredirect/\">");
        }

        [TestMethod]
        public async Task SignoutWithContext_AndConfirmation_RedirectsToDefaultWreply()
        {
            var client = GetClientWithFakeUserContext();
            var response = await client.GetAsync("/wsfederation?wa=wsignout1.0&wtrealm=urn:idsrv4:wsfed:sample");
            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);

            response = await client.GetAsync(response.Headers.Location);
            var content = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("/Account/Logout", response.RequestMessage.RequestUri.LocalPath);
            StringAssert.Contains(content, "Would you like to logout of IdentityServer?");

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var antiforgeryToken = doc.DocumentNode.SelectSingleNode("//input[@name='__RequestVerificationToken']").Attributes["value"].Value;
            var logoutId = doc.DocumentNode.SelectSingleNode("//input[@name='logoutId']").Attributes["value"].Value;

            var formPost = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", antiforgeryToken),
                new KeyValuePair<string, string>("logoutId", logoutId),
            });

            var postResponse = await client.PostAsync("/Account/Logout", formPost);
            var postContent = await postResponse.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, postResponse.StatusCode);
            StringAssert.Contains(postContent, "<a class=\"PostLogoutRedirectUri\" href=\"http://localhost:51214/\">");
        }
    }
}
