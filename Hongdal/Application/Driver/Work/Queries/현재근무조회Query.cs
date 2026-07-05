namespace Hongdal.Application.Driver.Work;

public sealed record 현재근무조회Query(string 기사Id) : IRequest<기사현재근무응답>;
