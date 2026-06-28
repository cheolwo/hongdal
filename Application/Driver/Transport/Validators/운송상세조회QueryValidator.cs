using FluentValidation;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송상세조회QueryValidator : AbstractValidator<운송상세조회Query>
{
    public 운송상세조회QueryValidator()
    {
        RuleFor(x => x.기사Id)
            .NotEmpty()
            .WithMessage("기사 인증 정보가 없습니다.");

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("운송 Id가 올바르지 않습니다.");
    }
}
