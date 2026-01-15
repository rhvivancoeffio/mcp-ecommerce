using MediatR;
using Domain.Sellers;

namespace Application.Sellers.Queries;

public record GetAvailableSellersQuery() : IRequest<GetAvailableSellersResponse>;

public record GetAvailableSellersResponse(
    IEnumerable<Seller> Sellers
);
