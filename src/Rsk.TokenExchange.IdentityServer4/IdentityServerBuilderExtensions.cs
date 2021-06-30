using Microsoft.Extensions.DependencyInjection;
using Rsk.TokenExchange.Configuration;
using Rsk.TokenExchange.Validators.Adaptors;

namespace Rsk.TokenExchange.IdentityServer4
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddTokenExchange(this IIdentityServerBuilder builder)
        {
            builder.Services.AddTokenExchangeCore();
            builder.Services.AddTransient<ITokenValidatorAdaptor, IdentityServerSubjectTokenValidator>();
            builder.AddExtensionGrantValidator<TokenExchangeExtensionGrantValidator>();
            
            return builder;
        }
    }
}