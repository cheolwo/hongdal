using Ssalddel.Contracts.Common.WorldProjection;

namespace Ssalddel.Application.Driver.Transport;

public sealed record 도심물류센터운송자Npc이동조회Query(string 기사Id)
    : IRequest<NpcMovementResponse?>;
