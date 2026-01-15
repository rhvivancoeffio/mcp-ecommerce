using System.ComponentModel;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Server.Helpers;
using Server.Common.Mcp.Attributes;

namespace Server.Checkout.McpResources;

// Note: Resources are registered manually in Program.cs - no [McpServerResourceType] needed
public sealed class CartHtmlResource
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CartHtmlResource> _logger;

    public CartHtmlResource(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CartHtmlResource> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    // Note: Resources are registered manually in Program.cs - no [McpServerResource] needed
    [OpenAiResourceMetadata(
        Uri = "ui://widget/cart.html",
        Title = "HTML widget view of the shopping cart",
        MimeType = "text/html+skybridge",
        InvokingMessage = "Cargando carrito...",
        InvokedMessage = "Carrito cargado.")]
    public ReadResourceResult Cart(CancellationToken cancellationToken = default)
    {
        // Obtener la URL base del servidor desde HttpContext
        var httpContext = _httpContextAccessor.HttpContext;
        var baseUrl = httpContext != null 
            ? $"https://{httpContext.Request.Host}"
            : "https://localhost:4444";

        // Encontrar los archivos compilados con hash
        var cartJs = StaticFileHelper.GetWidgetJsPath("cart");
        var mainCss = StaticFileHelper.GetAssetCssPath("main");
        var indexCss = StaticFileHelper.GetAssetCssPath("index");

        // Construir URLs absolutas
        var cartJsUrl = $"{baseUrl}{cartJs}";
        var mainCssUrl = $"{baseUrl}{mainCss}";
        var indexCssUrl = $"{baseUrl}{indexCss}";

        // Construir los links CSS
        var cssLinksHtml = $"<link rel=\"stylesheet\" href=\"{mainCssUrl}\"><link rel=\"stylesheet\" href=\"{indexCssUrl}\">";

        _logger.LogInformation("[CartHtmlResource] Generated URLs - Main CSS: {MainCss}, Index CSS: {IndexCss}, JS: {JsUrl}", mainCssUrl, indexCssUrl, cartJsUrl);

        // Generar HTML estático del widget con URLs absolutas (sin saltos de línea para evitar \n en JSON)
        var htmlContent = $"<!DOCTYPE html><html lang=\"en\" data-theme=\"light\"><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Cart Widget</title>{cssLinksHtml}</head><body><div id=\"root\"></div><script type=\"module\" src=\"{cartJsUrl}\"></script></body></html>";

        // Crear metadata para OpenAI
        var meta = new JsonObject
        {
            ["openai/outputTemplate"] = "ui://widget/cart.html",
            ["openai/widgetAccessible"] = true,
            ["openai/resultCanProduceWidget"] = true,
            ["openai/toolInvocation/invoking"] = "Cargando carrito...",
            ["openai/toolInvocation/invoked"] = "Carrito cargado."
        };

        // Retornar ReadResourceResult con TextResourceContents
        return new ReadResourceResult
        {
            Contents = new List<ResourceContents>
            {
                new TextResourceContents
                {
                    Uri = "ui://widget/cart.html",
                    MimeType = "text/html+skybridge",
                    Text = htmlContent,
                    Meta = meta
                }
            }
        };
    }
}
