using Ssalddel.Contracts.Common.WorldProjection;

namespace Ssalddel.Application.Driver.Transport;

public sealed record 도심물류센터운송자관점조회Query(string 기사Id)
    : IRequest<RolePerspectiveResponse?>;
