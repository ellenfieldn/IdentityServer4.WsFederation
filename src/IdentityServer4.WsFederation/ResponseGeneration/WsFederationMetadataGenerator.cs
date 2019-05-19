// Copyright (c) Nathan Ellenfield. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IdentityServer4.WsFederation
{
    public class WsFederationMetadataGenerator
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly IKeyMaterialService _keys;
        private static readonly WsFederationMetadataSerializer _serializer = new WsFederationMetadataSerializer();

        public WsFederationMetadataGenerator(ILogger<WsFederationMetadataGenerator> logger, IdentityServerOptions options, IKeyMaterialService keys)
        {
            _logger = logger;
            _options = options;
            _keys = keys;
        }

        public async Task<string> GetMetadata(HttpContext context)
        {
            var configuration = new WsFederationConfiguration
            {
                Issuer = _options.IssuerUri,
                SigningCredentials = await _keys.GetSigningCredentialsAsync(),
                TokenEndpoint = context.GetIdentityServerOrigin() + "/wsfederation"
            };
            //For whatever reason, the Digest method isn't specified in the builder extensions for identity server.
            //Not a good solution to force the user to use th eoverload that takes SigningCredentials
            //IdentityServer4/Configuration/DependencyInjection/BuilderExtensions/Crypto.cs
            //Instead, it should be supported in:
            //  The overload that takes a X509Certificate2
            //  The overload that looks it up in a cert store
            //  The overload that takes an RsaSecurityKey
            //  AddDeveloperSigningCredential
            //For now, this is a workaround.
            if (configuration.SigningCredentials.Digest == null)
            {
                _logger.LogInformation($"SigningCredentials does not have a digest specified. Using default digest algorithm of {SecurityAlgorithms.Sha256Digest}");
                configuration.SigningCredentials = new SigningCredentials(configuration.SigningCredentials.Key, configuration.SigningCredentials.Algorithm, SecurityAlgorithms.Sha256Digest);
            }
            configuration.KeyInfos.Add(new KeyInfo(configuration.SigningCredentials.Key));

            var sb = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true
            };
            using(var writer = XmlWriter.Create(sb, settings))
            {
                _serializer.WriteMetadata(writer, configuration);
            }
            return sb.ToString();
        }
    }
}
