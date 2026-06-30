using FluentResults;
using MediatR;

namespace Hongdal.Application.Driver.Notification;

public sealed record 기사Command기능설정수정Command(string 사용자Id, string CommandName, string FeatureName, bool IsEnabled) : IRequest<Result<Unit>>;
