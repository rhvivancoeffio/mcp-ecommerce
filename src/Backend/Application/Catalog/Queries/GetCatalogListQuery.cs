using MediatR;
using Domain.Catalog;

namespace Application.Catalog.Queries;

public record GetCatalogListQuery(
    string ShopKey,
    string? Category = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<GetCatalogListResponse>;

public record GetCatalogListResponse(
    IEnumerable<Product> Products,
    int TotalCount,
    int Page,
    int PageSize
);
