using Microsoft.Extensions.DependencyInjection;
using Rsk.TokenExchange.Configuration;
using Rsk.TokenExchange.Validators.Adaptors;

namespace Rsk.TokenExchange.DuendeIdentityServer
{
    /// <summary>
    /// Token exchange registrations for IIdentityServerBuilder
    /// </summary>
    public static class IdentityServerBuilderExtensions
    {
        /// <summary>
        /// Adds the default token exchange registrations for Duende IdentityServer.
        /// Uses the default authorization policy, designed for token exchange between microservices. 
        /// </summary>
        public static IIdentityServerBuilder AddTokenExchange(this IIdentityServerBuilder builder)
        {
            builder.Services.AddTokenExchangeCore();
            builder.Services.AddTransient<ITokenValidatorAdaptor, IdentityServerSubjectTokenValidator>();
            builder.AddExtensionGrantValidator<TokenExchangeExtensionGrantValidator>();
            
            return builder;
        }
    }
}