namespace Ssalddel.Application.Driver.Recommendation;

public sealed record 운송의뢰상세조회Query(string 기사Id, string RequestId) : IRequest<Ssalddel.Contracts.Driver.Recommendation.기사운송의뢰상세응답>;
