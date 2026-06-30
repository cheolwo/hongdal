using FluentResults;
using MediatR;

namespace Hongdal.Application.Abstractions;

public interface ICommand<TResult> : IRequest<Result<TResult>>;

public interface ICommand : IRequest<Result<Unit>>;

public interface IQuery<TResult> : IRequest<TResult>;
