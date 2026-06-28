using 홍달.도메인.배차;

namespace Hongdal.Application.Admin.Inbound;

public sealed record 배차대기단건조회Query(long Id) : IRequest<배차대기?>;
