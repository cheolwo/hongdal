using Hongdal.Contracts.CommandSettings;

namespace Hongdal.Application.Driver.Notification;

public sealed record 기사Command기능설정목록조회Query(string 사용자Id) : IRequest<Command기능설정목록응답>;
