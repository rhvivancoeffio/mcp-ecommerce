using MediatR;
using Domain.Catalog;

namespace Application.Catalog.Queries;

public class GetCatalogListQueryHandler : IRequestHandler<GetCatalogListQuery, GetCatalogListResponse>
{
    private readonly IProductRepository _productRepository;

    public GetCatalogListQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<GetCatalogListResponse> Handle(GetCatalogListQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Product> products;

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            products = await _productRepository.GetByCategoryAsync(request.Category, request.ShopKey, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            products = await _productRepository.SearchAsync(request.SearchTerm, request.ShopKey, cancellationToken);
        }
        else
        {
            products = await _productRepository.GetAllAsync(request.ShopKey, cancellationToken);
        }

        var productList = products.ToList();
        var totalCount = productList.Count;
        var pagedProducts = productList
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);

        return new GetCatalogListResponse(
            pagedProducts,
            totalCount,
            request.Page,
            request.PageSize
        );
    }
}
