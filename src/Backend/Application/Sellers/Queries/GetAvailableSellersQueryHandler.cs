using MediatR;
using Domain.Sellers;

namespace Application.Sellers.Queries;

public class GetAvailableSellersQueryHandler : IRequestHandler<GetAvailableSellersQuery, GetAvailableSellersResponse>
{
    private readonly ISellerRepository _sellerRepository;

    public GetAvailableSellersQueryHandler(ISellerRepository sellerRepository)
    {
        _sellerRepository = sellerRepository;
    }

    public async Task<GetAvailableSellersResponse> Handle(GetAvailableSellersQuery request, CancellationToken cancellationToken)
    {
        var sellers = await _sellerRepository.GetAllAsync(cancellationToken);
        return new GetAvailableSellersResponse(sellers);
    }
}
