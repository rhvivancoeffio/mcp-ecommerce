using System.Text.Json;
using Domain.Catalog;
using Infrastructure.Catalog.Vtex;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Catalog.Repositories;

public class VtexProductRepository : IProductRepository
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VtexProductRepository> _logger;

    public VtexProductRepository(
        HttpClient httpClient,
        ILogger<VtexProductRepository> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    private string BuildBaseUrl(string shopKey)
    {
        if (string.IsNullOrWhiteSpace(shopKey))
        {
            throw new ArgumentException("ShopKey is required for VTEX repository", nameof(shopKey));
        }
        
        return $"https://{shopKey}.vtexcommercestable.com.br/api/catalog_system/pub/products/search";
    }

    public async Task<IEnumerable<Product>> GetAllAsync(string shopKey, CancellationToken cancellationToken = default)
    {
        return await SearchAsync(string.Empty, shopKey, cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(Guid id, string shopKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await SearchAsync(id.ToString(), shopKey, cancellationToken);
            return products.FirstOrDefault(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VtexProductRepository] Error getting product by ID: {ProductId}", id);
            return null;
        }
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category, string shopKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = BuildBaseUrl(shopKey);
            var queryParams = $"?fq=C:/{category}/&O=OrderByPriceDESC";
            var url = $"{baseUrl}{queryParams}";
            
            _logger.LogInformation("[VtexProductRepository] Searching products by category: {Category} for shopKey: {ShopKey}", category, shopKey);
            return await FetchProductsFromVtex(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VtexProductRepository] Error getting products by category: {Category}", category);
            return Enumerable.Empty<Product>();
        }
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm, string shopKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = BuildBaseUrl(shopKey);
            var queryParams = string.IsNullOrWhiteSpace(searchTerm) 
                ? "?O=OrderByPriceDESC" 
                : $"?ft={Uri.EscapeDataString(searchTerm)}&O=OrderByPriceDESC";
            var url = $"{baseUrl}{queryParams}";
            
            _logger.LogInformation("[VtexProductRepository] Searching products with term: {SearchTerm} for shopKey: {ShopKey}", searchTerm, shopKey);
            return await FetchProductsFromVtex(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VtexProductRepository] Error searching products with term: {SearchTerm}", searchTerm);
            return Enumerable.Empty<Product>();
        }
    }

    private async Task<IEnumerable<Product>> FetchProductsFromVtex(string url, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var vtexProducts = JsonSerializer.Deserialize<List<VtexProductDto>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (vtexProducts == null || !vtexProducts.Any())
            {
                _logger.LogWarning("[VtexProductRepository] No products found from VTEX");
                return Enumerable.Empty<Product>();
            }

            var products = vtexProducts.Select(MapToProduct).Where(p => p != null).Cast<Product>().ToList();
            _logger.LogInformation("[VtexProductRepository] Mapped {Count} products from VTEX", products.Count);
            
            return products;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[VtexProductRepository] HTTP error fetching products from VTEX: {Url}", url);
            return Enumerable.Empty<Product>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[VtexProductRepository] JSON deserialization error");
            return Enumerable.Empty<Product>();
        }
    }

    private Product? MapToProduct(VtexProductDto vtexProduct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(vtexProduct.ProductId))
            {
                return null;
            }

            // Buscar el mejor item y seller con oferta comercial válida
            // Priorizar sellers con precio > 0 y stock > 0
            VtexItemDto? bestItem = null;
            VtexCommertialOfferDto? bestOffer = null;
            
            if (vtexProduct.Items != null && vtexProduct.Items.Any())
            {
                // Primera pasada: buscar ofertas con precio > 0 y stock > 0 (ideal)
                foreach (var item in vtexProduct.Items)
                {
                    if (item.Sellers != null)
                    {
                        foreach (var itemSeller in item.Sellers)
                        {
                            var itemOffer = itemSeller.CommertialOffer;
                            if (itemOffer != null && itemOffer.Price > 0 && itemOffer.AvailableQuantity > 0)
                            {
                                if (bestOffer == null || 
                                    itemOffer.Price > bestOffer.Price || 
                                    (itemOffer.Price == bestOffer.Price && itemOffer.AvailableQuantity > bestOffer.AvailableQuantity))
                                {
                                    bestItem = item;
                                    bestOffer = itemOffer;
                                }
                            }
                        }
                    }
                }
                
                // Segunda pasada: si no encontramos oferta ideal, buscar cualquier oferta con precio > 0
                if (bestOffer == null)
                {
                    foreach (var item in vtexProduct.Items)
                    {
                        if (item.Sellers != null)
                        {
                            foreach (var itemSeller in item.Sellers)
                            {
                                var itemOffer = itemSeller.CommertialOffer;
                                if (itemOffer != null && itemOffer.Price > 0)
                                {
                                    if (bestOffer == null || itemOffer.Price > bestOffer.Price)
                                    {
                                        bestItem = item;
                                        bestOffer = itemOffer;
                                    }
                                }
                            }
                        }
                    }
                }
                
                // Tercera pasada: si aún no encontramos nada, buscar cualquier oferta con stock > 0
                if (bestOffer == null)
                {
                    foreach (var item in vtexProduct.Items)
                    {
                        if (item.Sellers != null)
                        {
                            foreach (var itemSeller in item.Sellers)
                            {
                                var itemOffer = itemSeller.CommertialOffer;
                                if (itemOffer != null && itemOffer.AvailableQuantity > 0)
                                {
                                    if (bestOffer == null || itemOffer.AvailableQuantity > bestOffer.AvailableQuantity)
                                    {
                                        bestItem = item;
                                        bestOffer = itemOffer;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            var offer = bestOffer;
            
            // Si no hay oferta válida (precio > 0 o stock > 0), retornar null para filtrar el producto
            if (offer == null || (offer.Price <= 0 && offer.AvailableQuantity <= 0))
            {
                _logger.LogWarning("[VtexProductRepository] Product {ProductId} has no valid commercial offer", vtexProduct.ProductId);
                return null;
            }

            // Obtener SKU del mejor item o del productReference
            var sku = bestItem?.ItemId ?? vtexProduct.ProductReference ?? string.Empty;

            // Obtener imágenes: primero del mejor item, luego de otros items, luego de producto
            var imageUrls = new List<string>();
            if (bestItem?.Images != null && bestItem.Images.Any())
            {
                imageUrls.AddRange(bestItem.Images
                    .Where(img => !string.IsNullOrWhiteSpace(img.ImageUrl))
                    .Select(img => img.ImageUrl!)
                    .Distinct());
            }
            else if (vtexProduct.Items != null)
            {
                // Si el mejor item no tiene imágenes, buscar en otros items
                foreach (var item in vtexProduct.Items)
                {
                    if (item.Images != null && item.Images.Any())
                    {
                        imageUrls.AddRange(item.Images
                            .Where(img => !string.IsNullOrWhiteSpace(img.ImageUrl))
                            .Select(img => img.ImageUrl!)
                            .Distinct());
                        break; // Tomar imágenes del primer item que las tenga
                    }
                }
            }
            
            // Si aún no hay imágenes, usar las del producto
            if (!imageUrls.Any() && vtexProduct.Images != null && vtexProduct.Images.Any())
            {
                imageUrls.AddRange(vtexProduct.Images
                    .Where(img => !string.IsNullOrWhiteSpace(img.ImageUrl))
                    .Select(img => img.ImageUrl!)
                    .Distinct());
            }

            // Obtener primera imagen para ImageUrl (backward compatibility)
            var firstImageUrl = imageUrls.FirstOrDefault() ?? string.Empty;

            // Intentar parsear ProductId como Guid, si falla generar uno nuevo
            Guid productId;
            if (!Guid.TryParse(vtexProduct.ProductId, out productId))
            {
                // Generar Guid determinístico desde el ProductId string
                productId = GuidUtility.Create(vtexProduct.ProductId);
            }

            var product = new Product
            {
                Id = productId,
                Name = vtexProduct.ProductName ?? string.Empty,
                Description = vtexProduct.Description ?? string.Empty,
                Price = offer?.Price ?? 0m,
                SKU = sku,
                Category = vtexProduct.CategoryName ?? vtexProduct.CategoryId ?? string.Empty,
                Brand = vtexProduct.Brand ?? string.Empty,
                ImageUrl = firstImageUrl,
                ImageUrls = imageUrls,
                Stock = offer?.AvailableQuantity ?? 0,
                Attributes = new Dictionary<string, string>(),
                Features = new List<string>()
            };

            // Agregar referencias como atributos del mejor item
            if (bestItem?.ReferenceId != null)
            {
                foreach (var refId in bestItem.ReferenceId)
                {
                    if (!string.IsNullOrWhiteSpace(refId.Key) && !string.IsNullOrWhiteSpace(refId.Value))
                    {
                        product.Attributes[refId.Key] = refId.Value;
                    }
                }
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VtexProductRepository] Error mapping VTEX product: {ProductId}", vtexProduct.ProductId);
            return null;
        }
    }

    // Helper class para generar GUIDs determinísticos desde strings
    private static class GuidUtility
    {
        public static Guid Create(string value)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            var hash = System.Security.Cryptography.MD5.HashData(bytes);
            return new Guid(hash);
        }
    }
}
