using FluentResults;
using MediatR;

namespace Hongdal.Application.Driver.Notification;

public sealed record 기사Command기능설정기본값복원Command(string 사용자Id, string CommandName, string FeatureName) : IRequest<Result<Unit>>;
