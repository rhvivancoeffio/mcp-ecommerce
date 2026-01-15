using MediatR;

namespace Application.Catalog.Queries;

public record GetAvailableCategoriesQuery(string ShopKey) : IRequest<GetAvailableCategoriesResponse>;

public record GetAvailableCategoriesResponse(
    IEnumerable<string> Categories
);
