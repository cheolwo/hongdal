using Hongdal.Contracts.Shipper.Payment;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Application.Shipper.Payment;

public sealed class 토스결제환경조회QueryHandler : IRequestHandler<토스결제환경조회Query, 토스결제환경응답>
{
    private readonly IOptions<TossPaymentsOptions> _options;

    public 토스결제환경조회QueryHandler(IOptions<TossPaymentsOptions> options)
    {
        _options = options;
    }

    public Task<토스결제환경응답> Handle(토스결제환경조회Query request, CancellationToken cancellationToken)
    {
        return Task.FromResult(결제매퍼.To환경응답(_options.Value));
    }
}
