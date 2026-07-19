using FluentValidation;

namespace Ssalddel.Application.Driver.DispatchAction;

public sealed class 배차수락취소CommandValidator : AbstractValidator<배차수락취소Command>
{
    public 배차수락취소CommandValidator()
    {
        RuleFor(x => x.기사Id)
            .NotEmpty()
            .WithMessage("기사 인증 정보가 없습니다.");

        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("의뢰Id는 필수입니다.");

        RuleFor(x => x.사유)
            .MaximumLength(300)
            .WithMessage("수락 취소 사유는 300자 이하여야 합니다.");
    }
}
