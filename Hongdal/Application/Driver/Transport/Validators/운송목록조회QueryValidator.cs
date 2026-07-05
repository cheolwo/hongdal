using FluentValidation;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송목록조회QueryValidator : AbstractValidator<운송목록조회Query>
{
    public 운송목록조회QueryValidator()
    {
        RuleFor(x => x.기사Id)
            .NotEmpty()
            .WithMessage("기사 인증 정보가 없습니다.");
    }
}
