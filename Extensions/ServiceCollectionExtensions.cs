using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Function.RBAC.Authentication.Config;

namespace Azure.Function.RBAC.Authentication.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddJwtTokenValidator(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AuthConfig>(options => configuration.GetSection("AzureAd").Bind(options));
            services.AddScoped<IAuthentication, Authentication>();
        }
    }
}
