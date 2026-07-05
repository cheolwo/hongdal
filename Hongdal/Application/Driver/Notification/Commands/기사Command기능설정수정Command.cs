using FluentResults;
using MediatR;
using Hongdal.Application.Abstractions;
using 홍달.도메인.사용자;

namespace Hongdal.Application.Driver.Notification;

public sealed record 기사Command기능설정수정Command : 홍달CommandBase, IRequest<Result<Unit>>
{
    public 기사Command기능설정수정Command(string userId, string commandName, string featureName, bool isEnabled)
    {
        사용자Id = string.IsNullOrWhiteSpace(userId) ? string.Empty : userId;
        CommandName = commandName;
        FeatureName = featureName;
        IsEnabled = isEnabled;
        참여자Id = 사용자Id;
        실행역할 = 홍달역할유형.기사;
    }

    public string 사용자Id { get; init; } = string.Empty;
    public string CommandName { get; init; } = string.Empty;
    public string FeatureName { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}
