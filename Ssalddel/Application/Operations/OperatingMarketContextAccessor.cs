using Ssalddel.Contracts.Common.Operations;

namespace Ssalddel.Application.Operations;

public interface IOperatingMarketContextAccessor
{
    OperatingMarketContextSnapshot Current { get; }
}
