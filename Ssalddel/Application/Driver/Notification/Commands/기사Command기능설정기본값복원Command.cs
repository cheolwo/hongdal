using FluentResults;
using MediatR;
using Ssalddel.Application.Abstractions;
using 살뜰.도메인.사용자;

namespace Ssalddel.Application.Driver.Notification;

public sealed record 기사Command기능설정기본값복원Command : 살뜰CommandBase, IRequest<Result<Unit>>
{
    public 기사Command기능설정기본값복원Command(string userId, string commandName, string featureName)
    {
        사용자Id = string.IsNullOrWhiteSpace(userId) ? string.Empty : userId;
        CommandName = commandName;
        FeatureName = featureName;
        참여자Id = 사용자Id;
        실행역할 = 살뜰역할유형.기사;
    }

    public string 사용자Id { get; init; } = string.Empty;
    public string CommandName { get; init; } = string.Empty;
    public string FeatureName { get; init; } = string.Empty;
}
