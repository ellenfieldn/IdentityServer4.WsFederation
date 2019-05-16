using IdentityServer4.WsFederation;
using IdentityServer4.WsFederation.Validation;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WsFederationBuilderExtensions
    {
        public static IIdentityServerBuilder AddWsFederation(this IIdentityServerBuilder builder)
        {
            builder.Services.AddTransient<WsFederationMetadataGenerator>();
            builder.Services.AddTransient<IWsFederationRequestValidator, WsFederationRequestValidator>();
            builder.Services.AddTransient<IWsFederationResponseGenerator, WsFederationSigninResponseGenerator>();

            builder.AddEndpoint<WsFederationMetadataEndpoint>("Metadata", "/wsfederation/metadata");
            builder.AddEndpoint<WsFederationEndpoint>("Signin", "/wsfederation");
            return builder;
        }
    }
}
