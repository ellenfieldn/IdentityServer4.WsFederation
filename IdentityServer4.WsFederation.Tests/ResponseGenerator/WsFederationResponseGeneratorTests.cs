﻿using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.WsFederation.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace IdentityServer4.WsFederation.Tests.ResponseGenerator
{
    [TestClass]
    public class WsFederationResponseGeneratorTests
    {
        private DateTimeOffset _now = new DateTimeOffset(DateTime.UtcNow);

        private WsFederationSigninResponseGenerator GetDefaultResponseGenerator()
        {
            var clock = Substitute.For<ISystemClock>();
            clock.UtcNow.Returns(_now);

            var logger = Substitute.For<ILogger<WsFederationSigninResponseGenerator>>();

            var certificate = new X509Certificate2("IdentityServer4.WsFederation.Testing.pfx", "pw");
            var signingCredentials = new SigningCredentials(new X509SecurityKey(certificate), SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest);
            var keys = Substitute.For<IKeyMaterialService>();
            keys.GetSigningCredentialsAsync().Returns(signingCredentials);

            var options = new IdentityServerOptions
            {
                IssuerUri = "http://example.com/testissuer"
            };

            return new WsFederationSigninResponseGenerator(logger, clock, options, keys);
        }

        private ValidatedWsFederationRequest GetDefaultValidatedRequest()
        {
            var client = new Client
            {
                IdentityTokenLifetime = 300
            };
            var request = new ValidatedWsFederationRequest
            {
                Client = client,
                RequestMessage = new WsFederationMessage
                {
                    Wa = WsFederationConstants.WsFederationActions.SignIn,
                    Wctx = "Context",
                    Wreply = "http://example.com/mywreply",
                    Wtrealm = "http://example.com/myrealm"
                },
                Subject = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(IdentityModel.JwtClaimTypes.Name, "Bob Smith") }))
            };
            request.SetClient(client);
            return request;
        }

        [TestMethod]
        public void ResponseContainsRequest()
        {
            var generator = GetDefaultResponseGenerator();
            var request = GetDefaultValidatedRequest();
            var response = generator.GenerateResponseAsync(request).Result;
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Request);
            Assert.AreEqual(request, response.Request);
        }

        [TestMethod]
        public void ResponseHasCorrectIssuerAddress()
        {
            var generator = GetDefaultResponseGenerator();
            var request = GetDefaultValidatedRequest();
            var response = generator.GenerateResponseAsync(request).Result;
            Assert.AreEqual("http://example.com/mywreply", response.ResponseMessage.IssuerAddress);
        }

        [TestMethod]
        public void ResponseHasCorrectWa()
        {
            var generator = GetDefaultResponseGenerator();
            var request = GetDefaultValidatedRequest();
            var response = generator.GenerateResponseAsync(request).Result;
            Assert.AreEqual(WsFederationConstants.WsFederationActions.SignIn, response.ResponseMessage.Wa);
        }

        [TestMethod]
        public void ResponseHasCorrectWctx()
        {
            var generator = GetDefaultResponseGenerator();
            var request = GetDefaultValidatedRequest();
            var response = generator.GenerateResponseAsync(request).Result;
            Assert.AreEqual("Context", response.ResponseMessage.Wctx);
        }

        [TestMethod]
        public void ResponseHasCorrectWresult()
        {
            var generator = GetDefaultResponseGenerator();
            var request = GetDefaultValidatedRequest();
            var response = generator.GenerateResponseAsync(request).Result;
            StringAssert.Contains(response.ResponseMessage.Wresult, "RequestSecurityTokenResponse");
        }

        [TestMethod]
        public void RstrIsGenerated()
        {
            var generator = GetDefaultResponseGenerator();
            var request = GetDefaultValidatedRequest();
            var response = generator.GenerateSerializedRstr(request).Result;

            StringAssert.Contains(response, "RequestSecurityTokenResponse");
            StringAssert.Contains(response, "Lifetime");
            StringAssert.Contains(response, "AppliesTo");
            StringAssert.Contains(response, "TokenType");
            StringAssert.Contains(response, "RequestType");
            StringAssert.Contains(response, "KeyType");
        }

        [TestMethod]
        public void RstrIncludesToken()
        {
            var generator = GetDefaultResponseGenerator();
            var request = GetDefaultValidatedRequest();
            var response = generator.GenerateSerializedRstr(request).Result;

            StringAssert.Contains(response, "RequestedSecurityToken");
            StringAssert.Contains(response, "Assertion");
        }

        [TestMethod]
        public void GeneratedTokenHasValidSignature()
        {
            var generator = GetDefaultResponseGenerator();
            var request = GetDefaultValidatedRequest();
            var response = generator.GenerateSerializedRstr(request).Result;

            var tokenString = GetTokenString(response);
            var certificate = new X509Certificate2("IdentityServer4.WsFederation.Testing.pfx", "pw");
            var validationParams = new TokenValidationParameters
            {
                IssuerSigningKey = new X509SecurityKey(certificate),
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false,
            };
            var handler = new Saml2SecurityTokenHandler();
            
            handler.ValidateToken(tokenString, validationParams, out var validatedToken);
            Assert.IsNotNull(validatedToken);
        }

        private string GetTokenString(string rstr)
        {
            var doc = XDocument.Parse(rstr);
            XNamespace wstrust = "http://schemas.xmlsoap.org/ws/2005/02/trust";
            var requestedTokenElement = doc.Root.Element(wstrust + "RequestedSecurityToken");
            XNamespace assertionNs = "urn:oasis:names:tc:SAML:2.0:assertion";
            var assertion = requestedTokenElement.Element(assertionNs + "Assertion");
            return assertion.ToString(SaveOptions.DisableFormatting);
        }
    }
}
