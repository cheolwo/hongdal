using FluentResults;
using MediatR;
using 홍달.도메인.사용자;

namespace Hongdal.Application.Abstractions;

public interface ICommand<TResult> : IRequest<Result<TResult>>;

public interface ICommand : IRequest<Result<Unit>>;

public interface IQuery<TResult> : IRequest<TResult>;

public abstract record 홍달CommandBase
{
    public string 참여자Id { get; init; } = string.Empty;

    public 홍달역할유형 실행역할 { get; init; }

    public DateTimeOffset 요청시각 { get; init; } = DateTimeOffset.UtcNow;
}
