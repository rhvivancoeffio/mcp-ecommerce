using MediatR;
using Domain.Catalog;

namespace Application.Catalog.Queries;

public class GetAvailableCategoriesQueryHandler : IRequestHandler<GetAvailableCategoriesQuery, GetAvailableCategoriesResponse>
{
    private readonly IProductRepository _productRepository;

    public GetAvailableCategoriesQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<GetAvailableCategoriesResponse> Handle(GetAvailableCategoriesQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(request.ShopKey, cancellationToken);
        
        // Obtener categorías únicas y ordenadas
        var categories = products
            .Select(p => p.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToList();

        return new GetAvailableCategoriesResponse(categories);
    }
}
