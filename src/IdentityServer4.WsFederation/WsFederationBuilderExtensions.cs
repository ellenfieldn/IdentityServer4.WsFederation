using IdentityServer4.Services;
using IdentityServer4.WsFederation.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.WsFederation
{
    public static class WsFederationBuilderExtensions
    {
        public static IIdentityServerBuilder AddWsFederation(this IIdentityServerBuilder builder)
        {
            builder.Services.AddTransient<WsFederationMetadataGenerator>();
            builder.Services.AddTransient<IReturnUrlParser, WsFederationReturnUrlParser>();
            builder.Services.AddTransient<IWsFederationRequestValidator, WsFederationRequestValidator>();
            builder.Services.AddTransient<IWsFederationResponseGenerator, WsFederationSigninResponseGenerator>();

            builder.AddEndpoint<WsFederationMetadataEndpoint>("Metadata", "/wsfederation/metadata");
            builder.AddEndpoint<WsFederationSigninEndpoint>("Signin", "/wsfederation/signin");
            return builder;
        }
    }
}
