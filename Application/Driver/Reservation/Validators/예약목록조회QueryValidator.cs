using FluentValidation;

namespace Hongdal.Application.Driver.Reservation;

public sealed class 예약목록조회QueryValidator : AbstractValidator<예약목록조회Query>
{
    public 예약목록조회QueryValidator()
    {
        RuleFor(x => x.기사Id)
            .NotEmpty()
            .WithMessage("기사 인증 정보가 없습니다.");
    }
}
