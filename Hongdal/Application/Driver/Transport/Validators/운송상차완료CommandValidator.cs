using FluentValidation;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송상차완료CommandValidator : AbstractValidator<운송상차완료Command>
{
    public 운송상차완료CommandValidator()
    {
        RuleFor(x => x.기사Id)
            .NotEmpty()
            .WithMessage("기사 인증 정보가 없습니다.");

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("운송 Id가 올바르지 않습니다.");

        RuleFor(x => x.인수자명)
            .MaximumLength(100)
            .WithMessage("인수자명은 100자 이하여야 합니다.");

        RuleFor(x => x.인수증증빙방식)
            .MaximumLength(50)
            .WithMessage("인수증 증빙 방식은 50자 이하여야 합니다.");

        RuleFor(x => x.인수자소속)
            .MaximumLength(100)
            .WithMessage("인수자 소속은 100자 이하여야 합니다.");

        RuleFor(x => x.인수자서명)
            .MaximumLength(200)
            .WithMessage("인수자 서명은 200자 이하여야 합니다.");

        RuleFor(x => x.기사서명)
            .MaximumLength(200)
            .WithMessage("기사 서명은 200자 이하여야 합니다.");

        RuleFor(x => x.인수증서명생략사유)
            .MaximumLength(300)
            .WithMessage("인수증 서명 생략 사유는 300자 이하여야 합니다.");
    }
}
