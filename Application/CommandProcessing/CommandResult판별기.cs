using FluentResults;

namespace Hongdal.Application.CommandProcessing;

public static class CommandResult판별기
{
    public static bool TryGet성공여부<TResponse>(TResponse response, out bool isSuccess)
    {
        if (response is Result result)
        {
            isSuccess = result.IsSuccess;
            return true;
        }

        isSuccess = false;
        return false;
    }
}
