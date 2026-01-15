using MediatR;
using Domain.Catalog;

namespace Application.Catalog.Queries;

public class GetAvailableBrandsQueryHandler : IRequestHandler<GetAvailableBrandsQuery, GetAvailableBrandsResponse>
{
    private readonly IProductRepository _productRepository;

    public GetAvailableBrandsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<GetAvailableBrandsResponse> Handle(GetAvailableBrandsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(request.ShopKey, cancellationToken);
        
        // Obtener marcas únicas y ordenadas
        var brands = products
            .Select(p => p.Brand)
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(b => b)
            .ToList();

        return new GetAvailableBrandsResponse(brands);
    }
}
