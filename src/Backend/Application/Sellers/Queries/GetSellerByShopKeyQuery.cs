using MediatR;
using Domain.Sellers;

namespace Application.Sellers.Queries;

public record GetSellerByShopKeyQuery(string ShopKey) : IRequest<GetSellerByShopKeyResponse>;

public record GetSellerByShopKeyResponse(
    Seller? Seller
);
