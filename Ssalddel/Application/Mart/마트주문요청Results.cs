using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Ssalddel.Application.Mart;

internal static class 마트주문요청Results
{
    internal static Result<T> Unauthorized<T>()
        => Failure<T>("로그인 사용자 인증 정보가 필요합니다.", StatusCodes.Status401Unauthorized);

    internal static Result<T> BadRequest<T>(string message)
        => Failure<T>(message, StatusCodes.Status400BadRequest);

    internal static Result<T> NotFound<T>()
        => Failure<T>("마트 주문 요청을 찾을 수 없거나 현재 계정의 요청이 아닙니다.", StatusCodes.Status404NotFound);

    internal static Result<T> ProductNotFound<T>()
        => Failure<T>("공개된 마트 상품을 찾을 수 없습니다.", StatusCodes.Status404NotFound);

    internal static Result<T> Conflict<T>(string message)
        => Failure<T>(message, StatusCodes.Status409Conflict);

    private static Result<T> Failure<T>(string message, int statusCode)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", statusCode));
}
