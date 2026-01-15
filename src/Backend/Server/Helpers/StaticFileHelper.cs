using System.IO;

namespace Server.Helpers;

public static class StaticFileHelper
{
    private static string GetWwwRootPath()
    {
        // Try multiple paths to find wwwroot
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
        // Try base directory
        var wwwrootPath = Path.Combine(baseDirectory, "wwwroot");
        if (Directory.Exists(wwwrootPath))
        {
            return wwwrootPath;
        }
        
        // Try parent directory (for bin/Debug/net10.0 structure)
        var parentWwwrootPath = Path.Combine(baseDirectory, "..", "..", "..", "wwwroot");
        var resolvedParentPath = Path.GetFullPath(parentWwwrootPath);
        if (Directory.Exists(resolvedParentPath))
        {
            return resolvedParentPath;
        }
        
        // Fallback to base directory
        return wwwrootPath;
    }

    /// <summary>
    /// Encuentra un archivo en wwwroot/widgets/ que coincida con el patrón especificado
    /// </summary>
    public static string? FindWidgetFile(string pattern)
    {
        var wwwrootPath = GetWwwRootPath();
        var widgetsPath = Path.Combine(wwwrootPath, "widgets");
        if (!Directory.Exists(widgetsPath))
        {
            return null;
        }

        var files = Directory.GetFiles(widgetsPath);
        var matchingFiles = files
            .Where(f => Path.GetFileName(f).StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(f => File.GetLastWriteTime(f)) // Más reciente primero
            .ToList();

        // Si hay múltiples archivos con el mismo prefijo, devolver el más reciente
        var matchingFile = matchingFiles.FirstOrDefault();

        return matchingFile != null ? Path.GetFileName(matchingFile) : null;
    }

    /// <summary>
    /// Encuentra un archivo en wwwroot/assets/ que coincida con el patrón especificado
    /// </summary>
    public static string? FindAssetFile(string pattern, string? extension = null)
    {
        var wwwrootPath = GetWwwRootPath();
        var assetsPath = Path.Combine(wwwrootPath, "assets");
        if (!Directory.Exists(assetsPath))
        {
            return null;
        }

        var files = Directory.GetFiles(assetsPath);
        var matchingFiles = files
            .Where(f =>
            {
                var fileName = Path.GetFileName(f);
                // Buscar archivos que empiecen con el patrón
                var matchesPattern = fileName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase);
                
                // Si se especifica extensión, filtrar por extensión también
                if (!string.IsNullOrEmpty(extension))
                {
                    var fileExtension = Path.GetExtension(fileName);
                    return matchesPattern && fileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase);
                }
                
                return matchesPattern;
            })
            .OrderByDescending(f => File.GetLastWriteTime(f)) // Más reciente primero
            .ToList();

        // Si hay múltiples archivos con el mismo prefijo, devolver el más reciente
        var matchingFile = matchingFiles.FirstOrDefault();

        return matchingFile != null ? Path.GetFileName(matchingFile) : null;
    }

    /// <summary>
    /// Obtiene la ruta completa de un widget JS
    /// </summary>
    public static string GetWidgetJsPath(string widgetName)
    {
        var fileName = FindWidgetFile(widgetName);
        return fileName != null ? $"/widgets/{fileName}" : $"/widgets/{widgetName}.js";
    }

    /// <summary>
    /// Obtiene la ruta completa de un asset CSS
    /// </summary>
    public static string GetAssetCssPath(string assetName)
    {
        var fileName = FindAssetFile(assetName, ".css");
        return fileName != null ? $"/assets/{fileName}" : $"/assets/{assetName}.css";
    }
}
