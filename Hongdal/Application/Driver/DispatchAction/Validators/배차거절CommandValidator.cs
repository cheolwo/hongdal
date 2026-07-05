using FluentValidation;

namespace Hongdal.Application.Driver.DispatchAction;

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
    }
}
