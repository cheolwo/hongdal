using Hongdal.Contracts.Shipper.Payment;

namespace Hongdal.Application.Shipper.Payment;

public sealed record 토스결제환경조회Query() : IRequest<토스결제환경응답>;
