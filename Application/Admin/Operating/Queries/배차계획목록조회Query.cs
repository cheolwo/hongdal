using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Operating;

public sealed record 배차계획목록조회Query(string? 기사Id, string? 상태, DateTime? 신청일From, DateTime? 신청일To) : IRequest<IReadOnlyList<배차계획관리목록응답>>;
