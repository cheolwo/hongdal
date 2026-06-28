using Hongdal.Contracts.Driver.Settlement;

namespace Hongdal.Application.Driver.Settlement;

public sealed record 기사정산현재월조회Query(string 기사Id) : IRequest<기사정산응답>;
