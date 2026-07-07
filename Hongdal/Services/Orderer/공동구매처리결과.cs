using Microsoft.AspNetCore.Http;

namespace Hongdal.Services.Orderer;

public sealed class 공동구매처리결과<T>
{
    private 공동구매처리결과(bool 성공, int 상태코드, T? 값, string 메시지)
    {
        this.성공 = 성공;
        this.상태코드 = 상태코드;
        this.값 = 값;
        this.메시지 = 메시지;
    }

    public bool 성공 { get; }
    public int 상태코드 { get; }
    public T? 값 { get; }
    public string 메시지 { get; }

    public static 공동구매처리결과<T> 성공결과(T 값)
        => new(true, StatusCodes.Status200OK, 값, string.Empty);

    public static 공동구매처리결과<T> 잘못된요청(string 메시지, T? 값 = default)
        => new(false, StatusCodes.Status400BadRequest, 값, 메시지);

    public static 공동구매처리결과<T> 찾을수없음(string 메시지)
        => new(false, StatusCodes.Status404NotFound, default, 메시지);
}
