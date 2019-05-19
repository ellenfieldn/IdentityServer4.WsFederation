// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.WsFederation.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

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

        [TestMethod]
        public async Task MetadataPost_ReturnsBadMethod()
        {
            var response = await _client.PostAsync("/wsfederation/metadata", null);
            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [TestMethod]
        public async Task Metadata_CanBeDeserializedAsMetadata()
        {
            var response = await _client.GetAsync("/wsfederation/metadata");
            var content = await response.Content.ReadAsStreamAsync();

            using (var reader = XmlReader.Create(content))
            {
                var serializer = new WsFederationMetadataSerializer();
                var configuration = serializer.ReadMetadata(reader);
                Assert.AreEqual("http://localhost/wsfederation", configuration.TokenEndpoint);
            }
        }
    }
}
