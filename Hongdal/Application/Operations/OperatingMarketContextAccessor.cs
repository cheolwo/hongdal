using Hongdal.Contracts.Common.Operations;

namespace Hongdal.Application.Operations;

public interface IOperatingMarketContextAccessor
{
    OperatingMarketContextSnapshot Current { get; }
}
