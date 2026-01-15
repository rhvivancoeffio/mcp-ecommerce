using MediatR;

namespace Application.Catalog.Queries;

public record GetAvailableBrandsQuery(string ShopKey) : IRequest<GetAvailableBrandsResponse>;

public record GetAvailableBrandsResponse(
    IEnumerable<string> Brands
);
