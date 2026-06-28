namespace Hongdal.Application.Driver.Transport;

public sealed record 운송문제신고Command(string 기사Id, long Id, string 사유, string? 메모) : IRequest<Hongdal.Contracts.Driver.Transport.기사운송요약응답>;
