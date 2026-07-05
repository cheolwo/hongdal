using Hongdal.Contracts.Driver.Transport;

namespace Hongdal.Application.Driver.Transport;

public sealed record 운송상세조회Query(string 기사Id, long Id) : IRequest<기사운송상세응답?>;
