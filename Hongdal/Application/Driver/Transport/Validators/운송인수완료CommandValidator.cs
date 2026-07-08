using FluentValidation;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송인수완료CommandValidator : AbstractValidator<운송인수완료Command>
{
    public 운송인수완료CommandValidator()
    {
        RuleFor(x => x.기사Id)
            .NotEmpty()
            .WithMessage("기사 인증 정보가 없습니다.");

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("운송 Id가 올바르지 않습니다.");

        RuleFor(x => x.하차사진ObjectName)
            .MaximumLength(500)
            .WithMessage("하차 사진 저장 경로는 500자 이하여야 합니다.");

        RuleFor(x => x.하차사진Url)
            .MaximumLength(1000)
            .WithMessage("하차 사진 URL은 1000자 이하여야 합니다.");
    }
}
