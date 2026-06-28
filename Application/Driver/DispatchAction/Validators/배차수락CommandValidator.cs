using FluentValidation;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed class 배차수락CommandValidator : AbstractValidator<배차수락Command>
{
    public 배차수락CommandValidator()
    {
        RuleFor(x => x.기사Id)
            .NotEmpty()
            .WithMessage("기사 인증 정보가 없습니다.");

        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("의뢰Id는 필수입니다.");
    }
}
