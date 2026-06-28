using 홍달.도메인.배차;

namespace Hongdal.Application.Admin.Inbound;

public sealed record 배차대기목록조회Query() : IRequest<IReadOnlyList<배차대기>>;
