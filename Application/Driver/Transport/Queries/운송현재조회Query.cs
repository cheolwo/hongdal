using Hongdal.Contracts.Driver.Transport;

namespace Hongdal.Application.Driver.Transport;

public sealed record 운송현재조회Query(string 기사Id) : IRequest<기사운송요약응답>;
