using FluentValidation;

namespace Hongdal.Application.Driver.Reservation;

public sealed class 예약취소CommandValidator : AbstractValidator<예약취소Command>
{
    public 예약취소CommandValidator()
    {
        RuleFor(x => x.기사Id)
            .NotEmpty()
            .WithMessage("기사 인증 정보가 없습니다.");

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("예약 Id가 올바르지 않습니다.");
    }
}
