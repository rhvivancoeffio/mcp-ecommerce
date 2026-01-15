using System.ComponentModel;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Server.Helpers;
using Server.Common.Mcp.Attributes;

namespace Server.Catalog.McpResources;

// Note: Resources are registered manually in Program.cs - no [McpServerResourceType] needed
public sealed class ProductComparisonHtmlResource
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductComparisonHtmlResource(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Note: Resources are registered manually in Program.cs - no [McpServerResource] needed
    [OpenAiResourceMetadata(
        Uri = "ui://widget/product-comparison.html",
        Title = "HTML widget view for product comparison",
        MimeType = "text/html+skybridge",
        InvokingMessage = "Cargando comparación...",
        InvokedMessage = "Comparación cargada.")]
    public ReadResourceResult ProductComparison(
        CancellationToken cancellationToken = default)
    {
        // Obtener la URL base del servidor desde HttpContext
        var httpContext = _httpContextAccessor.HttpContext;
        var baseUrl = httpContext != null 
            ? $"https://{httpContext.Request.Host}"
            : "https://localhost:4444";

        // Encontrar los archivos compilados con hash
        var comparisonJs = StaticFileHelper.GetWidgetJsPath("product-comparison");
        var mainCss = StaticFileHelper.GetAssetCssPath("main");

        // Construir URLs absolutas
        var comparisonJsUrl = $"{baseUrl}{comparisonJs}";
        var mainCssUrl = $"{baseUrl}{mainCss}";

        // Generar HTML estático del widget con URLs absolutas (sin saltos de línea para evitar \n en JSON)
        // El script debe ser type=\"module\" porque usa import statements
        var htmlContent = $"<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Product Comparison Widget</title><link rel=\"stylesheet\" href=\"{mainCssUrl}\"></head><body><div id=\"root\"></div><script>window.openai = window.openai || {{}};</script><script type=\"module\" src=\"{comparisonJsUrl}\"></script></body></html>";

        // Crear metadata para OpenAI
        var meta = new JsonObject
        {
            ["openai/outputTemplate"] = "ui://widget/product-comparison.html",
            ["openai/widgetAccessible"] = true,
            ["openai/resultCanProduceWidget"] = true,
            ["openai/toolInvocation/invoking"] = "Cargando comparación...",
            ["openai/toolInvocation/invoked"] = "Comparación cargada."
        };

        // Retornar ReadResourceResult con TextResourceContents
        return new ReadResourceResult
        {
            Contents = new List<ResourceContents>
            {
                new TextResourceContents
                {
                    Uri = "ui://widget/product-comparison.html",
                    MimeType = "text/html+skybridge",
                    Text = htmlContent,
                    Meta = meta
                }
            }
        };
    }
}
