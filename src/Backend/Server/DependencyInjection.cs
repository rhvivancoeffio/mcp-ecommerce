using Microsoft.Extensions.DependencyInjection;
using Server.Catalog.McpTools;
using Server.Catalog.McpResources;
using Server.Checkout.McpTools;
using Server.Checkout.McpResources;

namespace Server;

public static class DependencyInjection
{
    public static IServiceCollection AddServerPresentation(this IServiceCollection services)
    {
        // Register HttpContextAccessor for Resources to get base URL
        services.AddHttpContextAccessor();
        
        // Register MCP Tools as scoped services
        // IMPORTANT: We need to register them manually so that:
        // 1. They are available for dependency injection
        // 2. Our custom ListToolsHandler can properly add _meta to them
        // WithToolsFromAssembly() will still discover them, but having them registered
        // ensures they're properly instantiated and available
        services.AddScoped<SellerListTool>();
        services.AddScoped<CatalogListTool>();
        services.AddScoped<ProductComparisonTool>();
        services.AddScoped<CategoryListTool>();
        services.AddScoped<BrandListTool>();
        services.AddScoped<AddToCartTool>();
        services.AddScoped<GetCartTool>();
        services.AddScoped<UpdateCartItemTool>();
        services.AddScoped<RemoveFromCartTool>();
        services.AddScoped<OpenCartWidgetTool>();
        
        // Register MCP Resources (necesitan IHttpContextAccessor para obtener la URL base)
        // Resources are registered as scoped because they use scoped services (IMediator, ISellerRepository, etc.)
        services.AddScoped<CatalogHtmlResource>();
        services.AddScoped<ProductComparisonHtmlResource>();
        services.AddScoped<CartHtmlResource>();
        
        return services;
    }
}
