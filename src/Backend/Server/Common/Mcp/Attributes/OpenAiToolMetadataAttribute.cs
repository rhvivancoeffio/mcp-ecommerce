using System;

namespace Server.Common.Mcp.Attributes;

/// <summary>
/// Atributo para agregar metadata de OpenAI al schema del tool
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class OpenAiToolMetadataAttribute : Attribute
{
    /// <summary>
    /// Nombre del tool MCP (ej: "catalog_list", "get_available_categories")
    /// </summary>
    public string ToolName { get; set; } = string.Empty;
    
    public string? OutputTemplate { get; set; }
    public bool WidgetAccessible { get; set; }
    public bool ResultCanProduceWidget { get; set; }
    public string Visibility { get; set; } = "public";
    public string InvokingMessage { get; set; } = "Ejecutando...";
    public string InvokedMessage { get; set; } = "Completado.";
}
