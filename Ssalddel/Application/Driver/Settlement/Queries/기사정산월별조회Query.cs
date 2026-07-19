using Ssalddel.Contracts.Driver.Settlement;

namespace Ssalddel.Application.Driver.Settlement;

public sealed record 기사정산월별조회Query(string 기사Id, int Year, int Month) : IRequest<기사정산응답?>;
