using MediatR;
using Domain.Catalog;

namespace Application.Catalog.Queries;

public class CompareProductsQueryHandler : IRequestHandler<CompareProductsQuery, CompareProductsResponse>
{
    private readonly IProductRepository _productRepository;

    public CompareProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<CompareProductsResponse> Handle(CompareProductsQuery request, CancellationToken cancellationToken)
    {
        var products = new List<Product>();
        var comparisonData = new Dictionary<string, Dictionary<Guid, object?>>();

        foreach (var productId in request.ProductIds)
        {
            var product = await _productRepository.GetByIdAsync(productId, request.ShopKey, cancellationToken);
            if (product != null)
            {
                products.Add(product);
            }
        }

        // Build comparison data
        foreach (var product in products)
        {
            comparisonData["Price"] = comparisonData.GetValueOrDefault("Price", new Dictionary<Guid, object?>());
            comparisonData["Price"][product.Id] = product.Price;

            comparisonData["Stock"] = comparisonData.GetValueOrDefault("Stock", new Dictionary<Guid, object?>());
            comparisonData["Stock"][product.Id] = product.Stock;

            comparisonData["Category"] = comparisonData.GetValueOrDefault("Category", new Dictionary<Guid, object?>());
            comparisonData["Category"][product.Id] = product.Category;

            foreach (var attribute in product.Attributes)
            {
                comparisonData[attribute.Key] = comparisonData.GetValueOrDefault(attribute.Key, new Dictionary<Guid, object?>());
                comparisonData[attribute.Key][product.Id] = attribute.Value;
            }
        }

        return new CompareProductsResponse(products, comparisonData);
    }
}
