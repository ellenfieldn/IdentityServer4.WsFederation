using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.WsFederation.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation.Tests
{
    [TestClass]
    public class WsFederationRequestValidatorTests
    {
        private ILogger<WsFederationSigninValidator> _logger = Substitute.For<ILogger<WsFederationSigninValidator>>();
        private IClientStore _clientStore = Substitute.For<IClientStore>();

        private WsFederationMessage GetDefaultWsFederationMessage() => new WsFederationMessage
        {
            Wa = WsFederationConstants.WsFederationActions.SignIn,
            Wtrealm = "http://wtrealm",
            Wreply = "http://wreply"
        };

        public WsFederationRequestValidatorTests()
        {
            var goodClient = new Client()
            {
                ProtocolType = IdentityServerConstants.ProtocolTypes.WsFederation,
                RedirectUris = new List<string> { "http://wreply" }
            };
            _clientStore.FindClientByIdAsync("http://wtrealm").Returns(goodClient);

            var noWsFedClient = new Client()
            {
                RedirectUris = new List<string> { "http://wreply" }
            };
            _clientStore.FindClientByIdAsync("http://noWsFedSupport").Returns(noWsFedClient);
        }

        [TestMethod]
        public async Task MissingWa_ReturnsError()
        {
            var message = GetDefaultWsFederationMessage();
            message.Wa = null;
            var requestValidator = new WsFederationSigninValidator(_logger, _clientStore);
            var response = await requestValidator.ValidateAsync(message, null);
            Assert.IsTrue(response.IsError);
            Assert.AreEqual("Missing wa", response.Error);
            Assert.AreEqual("No 'wa' was specified as part of the request.", response.ErrorDescription);
            Assert.AreEqual(message, response.ValidatedRequest.RequestMessage);
            Assert.IsNull(response.ValidatedRequest.Subject);
        }

        [TestMethod]
        public async Task MissingWtRealm_ReturnsError()
        {
            var message = GetDefaultWsFederationMessage();
            message.Wtrealm = null;
            var requestValidator = new WsFederationSigninValidator(_logger, _clientStore);
            var response = await requestValidator.ValidateAsync(message, null);
            Assert.IsTrue(response.IsError);
            Assert.AreEqual("Missing Wtrealm.", response.Error);
            Assert.AreEqual("Wtrealm was not passed in as a parameter.", response.ErrorDescription);
            Assert.AreEqual(message, response.ValidatedRequest.RequestMessage);
            Assert.IsNull(response.ValidatedRequest.Subject);
        }

        [TestMethod]
        public async Task MissingWReply_DefaultsToConfiguredWreply()
        {
            var message = GetDefaultWsFederationMessage();
            message.Wreply = null;
            var requestValidator = new WsFederationSigninValidator(_logger, _clientStore);
            var response = await requestValidator.ValidateAsync(message, null);
            Assert.IsFalse(response.IsError);
            Assert.IsNull(response.Error);
            Assert.IsNull(response.ErrorDescription);
            Assert.AreEqual(message, response.ValidatedRequest.RequestMessage);
            Assert.IsNull(response.ValidatedRequest.Subject);
            Assert.AreEqual("http://wreply", response.ValidatedRequest.RequestMessage.Wreply);
        }

        [TestMethod]
        public async Task InvalidClient_ReturnsError()
        {
            var message = GetDefaultWsFederationMessage();
            message.Wtrealm = "http://notARealClient";
            var requestValidator = new WsFederationSigninValidator(_logger, _clientStore);
            var response = await requestValidator.ValidateAsync(message, null);
            Assert.IsTrue(response.IsError);
            Assert.AreEqual("No Client.", response.Error);
            Assert.AreEqual("There is no client configured that matches the wtrealm parameter of the incoming request.", response.ErrorDescription);
            Assert.AreEqual(message, response.ValidatedRequest.RequestMessage);
            Assert.IsNull(response.ValidatedRequest.Subject);
        }

        [TestMethod]
        public async Task InvalidWReply_ReturnsError()
        {
            var message = GetDefaultWsFederationMessage();
            message.Wreply = "http://notARealWreply";
            var requestValidator = new WsFederationSigninValidator(_logger, _clientStore);
            var response = await requestValidator.ValidateAsync(message, null);
            Assert.IsTrue(response.IsError);
            Assert.AreEqual("Invalid redirect uri.", response.Error);
            Assert.AreEqual("The passed in redirect url is not valid for the given client.", response.ErrorDescription);
            Assert.AreEqual(message, response.ValidatedRequest.RequestMessage);
            Assert.IsNull(response.ValidatedRequest.Subject);
        }

        [TestMethod]
        public async Task ClientDoesNotSupportWsFederation_ReturnsError()
        {
            var message = GetDefaultWsFederationMessage();
            message.Wtrealm = "http://noWsFedSupport";
            var requestValidator = new WsFederationSigninValidator(_logger, _clientStore);
            var response = await requestValidator.ValidateAsync(message, null);
            Assert.IsTrue(response.IsError);
            Assert.AreEqual("Invalid protocol.", response.Error);
            Assert.AreEqual("The client identified by the wtrealm does not support WsFederation.", response.ErrorDescription);
            Assert.AreEqual(message, response.ValidatedRequest.RequestMessage);
            Assert.IsNull(response.ValidatedRequest.Subject);
        }

        [TestMethod]
        public async Task InvalidWa_ReturnsError()
        {
            var message = GetDefaultWsFederationMessage();
            message.Wa = "wtf";
            var requestValidator = new WsFederationSigninValidator(_logger, _clientStore);
            var response = await requestValidator.ValidateAsync(message, null);
            Assert.IsTrue(response.IsError);
            Assert.AreEqual("Unsupported action.", response.Error);
            Assert.AreEqual("wa=wtf is not supported.", response.ErrorDescription);
            Assert.AreEqual(message, response.ValidatedRequest.RequestMessage);
            Assert.IsNull(response.ValidatedRequest.Subject);
        }

        [TestMethod]
        public async Task WaSignin_ReturnsSuccess()
        {
            var message = GetDefaultWsFederationMessage();
            var requestValidator = new WsFederationSigninValidator(_logger, _clientStore);
            var response = await requestValidator.ValidateAsync(message, null);
            Assert.IsFalse(response.IsError);
            Assert.IsNull(response.Error);
            Assert.IsNull(response.ErrorDescription);
            Assert.AreEqual(message, response.ValidatedRequest.RequestMessage);
            Assert.IsNull(response.ValidatedRequest.Subject);
        }
    }
}
