using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers;

public static class Result응답확장
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return controller.ToFailureActionResult(result.Errors);
    }

    public static IActionResult ToNoContentActionResult(this ControllerBase controller, Result result)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        return controller.ToFailureActionResult(result.Errors);
    }

    public static IActionResult ToNoContentActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        return controller.ToFailureActionResult(result.Errors);
    }

    private static IActionResult ToFailureActionResult(this ControllerBase controller, IReadOnlyCollection<IError> errors)
    {
        var messages = errors.Select(x => x.Message).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        var firstMessage = messages.FirstOrDefault() ?? "요청을 처리할 수 없습니다.";
        var statusCode = 실패상태코드(firstMessage);
        var problem = new ProblemDetails
        {
            Title = firstMessage,
            Status = statusCode
        };
        problem.Extensions["errors"] = messages;

        return controller.StatusCode(statusCode, problem);
    }

    private static int 실패상태코드(string message)
    {
        if (message.Contains("찾을 수 없습니다", StringComparison.Ordinal))
        {
            return StatusCodes.Status404NotFound;
        }

        if (message.Contains("이미", StringComparison.Ordinal)
            || message.Contains("현재 상태", StringComparison.Ordinal)
            || message.Contains("수락할 수 없습니다", StringComparison.Ordinal))
        {
            return StatusCodes.Status409Conflict;
        }

        return StatusCodes.Status400BadRequest;
    }
}
