using Hongdal.Contracts.Driver.Transport;

namespace Hongdal.Application.Driver.Transport;

public sealed record 운송목록조회Query(string 기사Id) : IRequest<IReadOnlyList<기사운송요약응답>>;
