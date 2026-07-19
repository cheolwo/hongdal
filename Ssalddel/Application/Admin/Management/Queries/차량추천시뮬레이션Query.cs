using Ssalddel.Contracts.Admin.Management;

namespace Ssalddel.Application.Admin.Management;

public sealed record 차량추천시뮬레이션Query(차량추천시뮬레이션요청 요청) : IRequest<차량추천시뮬레이션응답>;
