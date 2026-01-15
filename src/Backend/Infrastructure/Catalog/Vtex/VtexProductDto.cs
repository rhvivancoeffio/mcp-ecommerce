using System.Text.Json.Serialization;

namespace Infrastructure.Catalog.Vtex;

/// <summary>
/// DTO para la respuesta de productos de VTEX API
/// </summary>
public class VtexProductDto
{
    [JsonPropertyName("productId")]
    public string? ProductId { get; set; }

    [JsonPropertyName("productName")]
    public string? ProductName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("brand")]
    public string? Brand { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }

    [JsonPropertyName("linkText")]
    public string? LinkText { get; set; }

    [JsonPropertyName("productReference")]
    public string? ProductReference { get; set; }

    [JsonPropertyName("categoryId")]
    public string? CategoryId { get; set; }

    [JsonPropertyName("categoryName")]
    public string? CategoryName { get; set; }

    [JsonPropertyName("items")]
    public List<VtexItemDto>? Items { get; set; }

    [JsonPropertyName("images")]
    public List<VtexImageDto>? Images { get; set; }
}

public class VtexItemDto
{
    [JsonPropertyName("itemId")]
    public string? ItemId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("nameComplete")]
    public string? NameComplete { get; set; }

    [JsonPropertyName("referenceId")]
    public List<VtexReferenceIdDto>? ReferenceId { get; set; }

    [JsonPropertyName("images")]
    public List<VtexImageDto>? Images { get; set; }

    [JsonPropertyName("sellers")]
    public List<VtexSellerDto>? Sellers { get; set; }
}

public class VtexReferenceIdDto
{
    [JsonPropertyName("Key")]
    public string? Key { get; set; }

    [JsonPropertyName("Value")]
    public string? Value { get; set; }
}

public class VtexImageDto
{
    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("imageTag")]
    public string? ImageTag { get; set; }
}

public class VtexSellerDto
{
    [JsonPropertyName("sellerId")]
    public string? SellerId { get; set; }

    [JsonPropertyName("sellerName")]
    public string? SellerName { get; set; }

    [JsonPropertyName("commertialOffer")]
    public VtexCommertialOfferDto? CommertialOffer { get; set; }
}

public class VtexCommertialOfferDto
{
    [JsonPropertyName("Price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("ListPrice")]
    public decimal? ListPrice { get; set; }

    [JsonPropertyName("AvailableQuantity")]
    public int? AvailableQuantity { get; set; }
}
