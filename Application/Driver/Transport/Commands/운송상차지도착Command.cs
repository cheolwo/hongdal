using Hongdal.Contracts.Driver.Transport;
using FluentResults;

namespace Hongdal.Application.Driver.Transport;

public sealed record 운송상차지도착Command(string 기사Id, long Id) : IRequest<Result<기사운송상태변경응답>>;
