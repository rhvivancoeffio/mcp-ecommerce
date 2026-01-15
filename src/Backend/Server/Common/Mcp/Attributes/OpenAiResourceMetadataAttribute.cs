using System;

namespace Server.Common.Mcp.Attributes;

/// <summary>
/// Atributo para agregar metadata de OpenAI al schema del resource
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class OpenAiResourceMetadataAttribute : Attribute
{
    /// <summary>
    /// URI del resource (ej: "ui://widget/catalog.html")
    /// </summary>
    public string Uri { get; set; } = string.Empty;
    
    public string? Title { get; set; }
    public string MimeType { get; set; } = "text/html+skybridge";
    public string InvokingMessage { get; set; } = "Cargando...";
    public string InvokedMessage { get; set; } = "Recurso cargado.";
}
