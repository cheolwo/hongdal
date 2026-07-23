using Microsoft.Extensions.Logging;
using Ssalddel.Application.Community;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Application.Behaviors;

public sealed class CommunityActivityCommandPostBehavior<TRequest, TResponse>(
    ICommunityActivityPostPublisher publisher,
    ILogger<CommunityActivityCommandPostBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly string[] SuccessPropertyNames =
    [
        "IsSuccess",
        "Success",
        "성공여부",
        "완료여부"
    ];

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var definition = CommunityActivityBoardCatalog.FindSource(
            CommunityActivitySourceKinds.Command,
            typeof(TRequest).Name);
        var response = await next();
        if (definition is null || !IsSuccessfulResponse(response))
        {
            return response;
        }

        try
        {
            await publisher.PublishAsync(definition, request, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                exception,
                "Command 활동 게시글 발행에 실패했습니다. CommandName={CommandName} BoardKey={BoardKey}",
                typeof(TRequest).Name,
                definition.Board.Key);
        }

        return response;
    }

    internal static bool IsSuccessfulResponse(TResponse response)
    {
        if (response is null)
        {
            return false;
        }

        if (response is bool booleanResponse)
        {
            return booleanResponse;
        }

        foreach (var propertyName in SuccessPropertyNames)
        {
            var successProperty = response.GetType().GetProperty(propertyName);
            if (successProperty?.PropertyType == typeof(bool))
            {
                return (bool)(successProperty.GetValue(response) ?? false);
            }
        }

        return true;
    }
}
