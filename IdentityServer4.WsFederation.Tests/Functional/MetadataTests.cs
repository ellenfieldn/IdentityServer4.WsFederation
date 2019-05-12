using IdentityServer4.WsFederation.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation.Tests.Functional
{
    [TestClass]
    public class MetadataTests
    {
        private readonly HttpClient _client;

        public MetadataTests()
        {
            var factory = new WebApplicationFactory<Program>();
            _client = factory.CreateClient();
        }

        [TestMethod]
        public async Task MetadataSuccess()
        {
            var response = await _client.GetAsync("/wsfederation/metadata");
            var content = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StringAssert.Contains(content, "<md:EntityDescriptor entityID=\"urn:idsrv4:wsfed:server:sample\" xmlns:md=\"urn:oasis:names:tc:SAML:2.0:metadata\">");
        }
    }
}
