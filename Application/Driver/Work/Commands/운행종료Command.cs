namespace Hongdal.Application.Driver.Work;

public sealed record 운행종료Command(string 기사Id) : IRequest<Unit>;
