using Hongdal.Contracts.Driver.Work;

namespace Hongdal.Application.Driver.Work;

public sealed record 기사근무상세조회Query(string 기사Id, long Id) : IRequest<기사근무요약응답?>;
