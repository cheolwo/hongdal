using Hongdal.Application.Driver.Transport;

namespace Hongdal.Tests.Application.Driver.Transport;

public class 운송문제신고CommandValidatorTests
{
    private readonly 운송문제신고CommandValidator _validator = new();

    [Fact]
    public void Validate_사유만_있어도_기존_문제신고를_허용한다()
    {
        var command = new 운송문제신고Command("driver-1", 10, "현장 문제", "담당자 통화 중");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_예외코드만_있어도_구조화된_예외신고를_허용한다()
    {
        var command = new 운송문제신고Command(
            "driver-1",
            10,
            "상차",
            "상차담당자부재",
            null,
            null,
            null,
            null,
            true);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_사유와_예외코드가_모두_없으면_실패한다()
    {
        var command = new 운송문제신고Command(
            "driver-1",
            10,
            "상차",
            null,
            null,
            null,
            null,
            null,
            false);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "문제 사유 또는 예외 코드는 필수입니다.");
    }
}
