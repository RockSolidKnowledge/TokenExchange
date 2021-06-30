using Microsoft.Extensions.DependencyInjection;
using Rsk.TokenExchange.Validators;

namespace Rsk.TokenExchange.Configuration
{
    /// <summary>
    /// Extensions for MS DI.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the default, core (non-IdentityServer) implementations for token exchange.
        /// </summary>
        public static IServiceCollection AddTokenExchangeCore(this IServiceCollection services)
        {
            services.AddTransient<ITokenExchangeRequestParser, TokenExchangeRequestParser>();
            services.AddTransient<ITokenExchangeRequestValidator, DefaultTokenExchangeRequestValidator>();
            services.AddTransient<ISubjectTokenValidator, DefaultSubjectTokenValidator>();
            services.AddTransient<ITokenExchangeClaimsParser, TokenExchangeClaimsParser>();
            
            return services;
        }
    }
}