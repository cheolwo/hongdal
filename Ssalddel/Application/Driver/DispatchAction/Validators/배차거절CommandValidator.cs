using FluentValidation;

namespace Ssalddel.Application.Driver.DispatchAction;

public sealed class 배차거절CommandValidator : AbstractValidator<배차거절Command>
{
    public 배차거절CommandValidator()
    {
        RuleFor(x => x.기사Id)
            .NotEmpty()
            .WithMessage("기사 인증 정보가 없습니다.");

        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("의뢰Id는 필수입니다.");

        RuleFor(x => x.사유)
            .MaximumLength(300)
            .WithMessage("거절 사유는 300자 이하여야 합니다.");
    }
}
