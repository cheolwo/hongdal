namespace Hongdal.Application.Driver.Work;

public sealed record 운행상태조회Query(string 기사Id) : IRequest<기사운행상태응답>;
