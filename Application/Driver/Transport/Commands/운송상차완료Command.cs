using Hongdal.Contracts.Driver.Transport;

namespace Hongdal.Application.Driver.Transport;

public sealed record 운송상차완료Command(string 기사Id, long Id) : IRequest<기사운송상태변경응답>;
