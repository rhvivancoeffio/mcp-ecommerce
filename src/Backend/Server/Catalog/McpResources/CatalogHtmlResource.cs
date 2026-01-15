using System.ComponentModel;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MediatR;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Application.Catalog.Queries;
using Domain.Sellers;
using Server.Helpers;
using Server.Common.Mcp.Attributes;

namespace Server.Catalog.McpResources;

// Note: Resources are registered manually in Program.cs - no [McpServerResourceType] needed
public sealed class CatalogHtmlResource
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CatalogHtmlResource> _logger;
    private readonly IMediator _mediator;
    private readonly ISellerRepository _sellerRepository;

    public CatalogHtmlResource(
        IHttpContextAccessor httpContextAccessor, 
        ILogger<CatalogHtmlResource> logger,
        IMediator mediator,
        ISellerRepository sellerRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _mediator = mediator;
        _sellerRepository = sellerRepository;
    }

    // Note: Resources are registered manually in Program.cs - no [McpServerResource] needed
    [OpenAiResourceMetadata(
        Uri = "ui://widget/catalog.html",
        Title = "HTML widget view of the product catalog",
        MimeType = "text/html+skybridge",
        InvokingMessage = "Cargando catálogo...",
        InvokedMessage = "Catálogo cargado.")]
    public async Task<ReadResourceResult> Catalog(CancellationToken cancellationToken = default)
    {
        // Obtener la URL base del servidor desde HttpContext
        var httpContext = _httpContextAccessor.HttpContext;
        var baseUrl = httpContext != null 
            ? $"https://{httpContext.Request.Host}"
            : "https://localhost:4444";

        // Encontrar los archivos compilados con hash
        var catalogJs = StaticFileHelper.GetWidgetJsPath("catalog");
        var catalogCss = StaticFileHelper.GetAssetCssPath("catalog");
        var indexCss = StaticFileHelper.GetAssetCssPath("index");

        // Construir URLs absolutas
        var catalogJsUrl = $"{baseUrl}{catalogJs}";
        var catalogCssUrl = $"{baseUrl}{catalogCss}";
        var indexCssUrl = $"{baseUrl}{indexCss}";
        
        // Construir los links CSS
        var cssLinksHtml = $"<link rel=\"stylesheet\" href=\"{catalogCssUrl}\"><link rel=\"stylesheet\" href=\"{indexCssUrl}\">";

        // Obtener el primer seller disponible por defecto (para recursos estáticos)
        var sellers = await _sellerRepository.GetAllAsync(cancellationToken);
        var defaultSeller = sellers.FirstOrDefault();
        
        List<string> categories;
        List<string> brands;
        
        if (defaultSeller == null)
        {
            _logger.LogWarning("[CatalogHtmlResource] No sellers available, returning empty categories and brands");
            categories = new List<string>();
            brands = new List<string>();
        }
        else
        {
            // Obtener categorías y marcas disponibles usando el shopKey del primer seller
            var categoriesQuery = new GetAvailableCategoriesQuery(defaultSeller.ShopKey);
            var categoriesResponse = await _mediator.Send(categoriesQuery, cancellationToken);
            
            var brandsQuery = new GetAvailableBrandsQuery(defaultSeller.ShopKey);
            var brandsResponse = await _mediator.Send(brandsQuery, cancellationToken);
            
            categories = categoriesResponse.Categories.ToList();
            brands = brandsResponse.Brands.ToList();
        }

        // Preparar datos iniciales para el widget
        var initialData = new
        {
            categories = categories,
            brands = brands
        };

        // Serializar datos iniciales a JSON (escapar para HTML/JavaScript)
        var initialDataJson = JsonSerializer.Serialize(initialData, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var escapedInitialDataJson = initialDataJson
            .Replace("\\", "\\\\")
            .Replace("</script>", "<\\/script>")
            .Replace("\"", "\\\"");
        
        _logger.LogInformation("[CatalogHtmlResource] Generated URLs - Catalog CSS: {CatalogCss}, Index CSS: {IndexCss}, JS: {JsUrl}", catalogCssUrl, indexCssUrl, catalogJsUrl);
        _logger.LogInformation("[CatalogHtmlResource] Loaded {CategoryCount} categories and {BrandCount} brands", categories.Count, brands.Count);

        // Generar HTML estático del widget
        var htmlContent = $"<!DOCTYPE html><html lang=\"en\" data-theme=\"light\"><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Catalog Widget</title>{cssLinksHtml}</head><body><div id=\"root\"></div><script>window.openai = window.openai || {{}};window.openai.initialData = window.openai.initialData || {escapedInitialDataJson};</script><script type=\"module\" src=\"{catalogJsUrl}\"></script></body></html>";

        // Crear metadata para OpenAI
        var meta = new JsonObject
        {
            ["openai/outputTemplate"] = "ui://widget/catalog.html",
            ["openai/widgetAccessible"] = true,
            ["openai/resultCanProduceWidget"] = true,
            ["openai/toolInvocation/invoking"] = "Cargando catálogo...",
            ["openai/toolInvocation/invoked"] = "Catálogo cargado."
        };

        // Retornar ReadResourceResult con TextResourceContents
        return new ReadResourceResult
        {
            Contents = new List<ResourceContents>
            {
                new TextResourceContents
                {
                    Uri = "ui://widget/catalog.html",
                    MimeType = "text/html+skybridge",
                    Text = htmlContent,
                    Meta = meta
                }
            }
        };
    }
}
