using IdentityServer4.WsFederation.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation.Tests.Functional
{
    [TestClass]
    public class SigninTests
    {
        private readonly  HttpClient _client;

        public SigninTests()
        {
            var factory = new WebApplicationFactory<Program>();
            _client = factory.CreateClient();
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
    }
}
