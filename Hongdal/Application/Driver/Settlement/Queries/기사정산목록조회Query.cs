using Hongdal.Contracts.Driver.Settlement;

namespace Hongdal.Application.Driver.Settlement;

public sealed record 기사정산목록조회Query(string 기사Id) : IRequest<IReadOnlyList<기사정산월요약응답>>;
