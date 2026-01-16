using MediatR;
using Domain.Sellers;

namespace Application.Sellers.Queries;

public class GetSellerByShopKeyQueryHandler : IRequestHandler<GetSellerByShopKeyQuery, GetSellerByShopKeyResponse>
{
    private readonly ISellerRepository _sellerRepository;

    public GetSellerByShopKeyQueryHandler(ISellerRepository sellerRepository)
    {
        _sellerRepository = sellerRepository;
    }

    public async Task<GetSellerByShopKeyResponse> Handle(GetSellerByShopKeyQuery request, CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository.GetByShopKeyAsync(request.ShopKey, cancellationToken);
        return new GetSellerByShopKeyResponse(seller);
    }
}
