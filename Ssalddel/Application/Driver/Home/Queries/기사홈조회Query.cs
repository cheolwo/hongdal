using Ssalddel.Contracts.Driver.Home;

namespace Ssalddel.Application.Driver.Home;

public sealed record 기사홈조회Query(string 기사Id) : IRequest<기사홈요약응답?>;
