using MediatR;
using Domain.Catalog;

namespace Application.Catalog.Queries;

public record CompareProductsQuery(
    IEnumerable<Guid> ProductIds,
    string ShopKey
) : IRequest<CompareProductsResponse>;

public record CompareProductsResponse(
    IEnumerable<Product> Products,
    Dictionary<string, Dictionary<Guid, object?>> ComparisonData
);
