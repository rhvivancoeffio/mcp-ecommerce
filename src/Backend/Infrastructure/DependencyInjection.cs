using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Domain.Catalog;
using Domain.Sellers;
using Domain.Checkout;
using Infrastructure.Catalog.Repositories;
using Infrastructure.Sellers.Repositories;
using Infrastructure.Checkout.Repositories;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Registrar repositorio de sellers
        services.AddSingleton<ISellerRepository, InMemorySellerRepository>();
        
        // Registrar repositorio de carritos
        services.AddSingleton<ICartRepository, InMemoryCartRepository>();
        
        // Determinar qué repositorio usar según configuración
        var repositoryType = configuration?["ProductRepository:Type"] ?? "InMemory";
        
        if (repositoryType.Equals("Vtex", StringComparison.OrdinalIgnoreCase))
        {
            // Configurar HttpClient para VTEX
            services.AddHttpClient<VtexProductRepository>(client =>
            {
                // Configuración básica del HttpClient
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            
            // Registrar VtexProductRepository sin shopKey (se pasa dinámicamente en cada método)
            services.AddScoped<IProductRepository>(serviceProvider =>
            {
                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(nameof(VtexProductRepository));
                var logger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<VtexProductRepository>>();
                return new VtexProductRepository(httpClient, logger);
            });
        }
        else
        {
            // Usar repositorio en memoria por defecto
            services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        }
        
        return services;
    }
}
