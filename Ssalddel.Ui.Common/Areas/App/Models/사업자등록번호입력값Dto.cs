namespace Ssalddel.Ui.Common.Areas.App.Models;

public sealed class 사업자등록번호입력값Dto
{
    public string 원본값 { get; set; } = string.Empty;

    public string 숫자값 { get; set; } = string.Empty;

    public string 표시값 { get; set; } = string.Empty;

    public bool 형식유효 { get; set; }

    public bool 체크섬유효 { get; set; }
}
