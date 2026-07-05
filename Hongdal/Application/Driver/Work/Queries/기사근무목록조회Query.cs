using Hongdal.Contracts.Driver.Work;

namespace Hongdal.Application.Driver.Work;

public sealed record 기사근무목록조회Query(string 기사Id) : IRequest<IReadOnlyList<기사근무요약응답>>;
