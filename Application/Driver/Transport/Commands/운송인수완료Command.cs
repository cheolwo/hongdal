using Hongdal.Contracts.Driver.Transport;
using FluentResults;

namespace Hongdal.Application.Driver.Transport;

public sealed record 운송인수완료Command(string 기사Id, long Id) : IRequest<Result<기사운송상태변경응답>>;
