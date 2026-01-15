using System.ComponentModel;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Server.Helpers;
using Server.Common.Mcp.Attributes;

namespace Server.Catalog.McpResources;

// Note: Resources are registered manually in Program.cs - no [McpServerResourceType] needed
public sealed class CatalogHtmlResource
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CatalogHtmlResource(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Note: Resources are registered manually in Program.cs - no [McpServerResource] needed
    [OpenAiResourceMetadata(
        Uri = "ui://widget/catalog.html",
        Title = "HTML widget view of the product catalog",
        MimeType = "text/html+skybridge",
        InvokingMessage = "Cargando catálogo...",
        InvokedMessage = "Catálogo cargado.")]
    public ReadResourceResult Catalog(CancellationToken cancellationToken = default)
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

        // Generar HTML estático del widget
        var htmlContent = $"<!DOCTYPE html><html lang=\"en\" data-theme=\"light\"><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Catalog Widget</title>{cssLinksHtml}</head><body><div id=\"root\"></div><script>window.openai = window.openai || {{}};</script><script type=\"module\" src=\"{catalogJsUrl}\"></script></body></html>";

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
