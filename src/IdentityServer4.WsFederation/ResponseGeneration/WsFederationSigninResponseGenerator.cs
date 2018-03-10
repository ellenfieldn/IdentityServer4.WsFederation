using IdentityServer4.Configuration;
using IdentityServer4.Services;
using IdentityServer4.WsFederation.Validation;
using IdentityServer4.WsFederation.WsTrust.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.WsFederation
{
    public class WsFederationSigninResponseGenerator : IWsFederationResponseGenerator
    {
        private readonly ILogger _logger;
        private readonly ISystemClock _clock;
        private readonly IdentityServerOptions _options;
        private readonly IKeyMaterialService _keys;
        
        public WsFederationSigninResponseGenerator(ILogger<WsFederationSigninResponseGenerator> logger, ISystemClock clock, IdentityServerOptions options, IKeyMaterialService keys)
        {
            _logger = logger;
            _clock = clock;
            _options = options;
            _keys = keys;
        }

        public async Task<WsFederationSigninResponse> GenerateResponseAsync(ValidatedWsFederationRequest request)
        {
            _logger.LogDebug("Creating WsFederation Signin Response.");
            var responseMessage = new WsFederationMessage
            {
                IssuerAddress = request.RequestMessage.Wreply,
                Wa = request.RequestMessage.Wa,
                Wctx = request.RequestMessage.Wctx,
                Wresult = await GenerateSerializedRstr(request)
            };

            var response = new WsFederationSigninResponse
            {
                Request = request,
                ResponseMessage = responseMessage
            };
            return response;
        }

        public async Task<string> GenerateSerializedRstr(ValidatedWsFederationRequest request)
        {
            var now = _clock.UtcNow.UtcDateTime;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = request.RequestMessage.Wtrealm,
                Expires = now.AddSeconds(request.Client.IdentityTokenLifetime),
                IssuedAt = now,
                Issuer = _options.IssuerUri,
                NotBefore = now,
                SigningCredentials = await _keys.GetSigningCredentialsAsync(),
                Subject = request.Subject.Identity as ClaimsIdentity
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
            if (tokenDescriptor.SigningCredentials.Digest == null)
            {
                _logger.LogInformation($"SigningCredentials does not have a digest specified. Using default digest algorithm of {SecurityAlgorithms.Sha256Digest}");
                tokenDescriptor.SigningCredentials = new SigningCredentials(tokenDescriptor.SigningCredentials.Key, tokenDescriptor.SigningCredentials.Algorithm, SecurityAlgorithms.Sha256Digest);
            }

            _logger.LogDebug("Creating SAML 2.0 security token.");
            var tokenHandler = new Saml2SecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            _logger.LogDebug("Serializing RSTR.");
            var rstr = new RequestSecurityTokenResponse
            {
                AppliesTo = new AppliesTo(request.RequestMessage.Wtrealm),
                KeyType = "http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey",
                Lifetime = new Lifetime
                {
                    Created = now.ToString(),
                    Expires = now.AddSeconds(request.Client.IdentityTokenLifetime).ToString(),
                },
                RequestedSecurityToken = token,
                RequestType = "http://schemas.xmlsoap.org/ws/2005/02/trust/Issue",
                TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0"
            };
            return RequestSecurityTokenResponseSerializer.Serialize(rstr);
        }
    }
}
