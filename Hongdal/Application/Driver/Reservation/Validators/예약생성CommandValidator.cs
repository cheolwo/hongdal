using FluentValidation;

namespace Hongdal.Application.Driver.Reservation;

public sealed class 예약생성CommandValidator : AbstractValidator<예약생성Command>
{
    public 예약생성CommandValidator()
    {
        RuleFor(x => x.기사Id)
            .NotEmpty()
            .WithMessage("기사 인증 정보가 없습니다.");

        RuleFor(x => x.시작모드)
            .NotEmpty()
            .WithMessage("시작모드는 필수입니다.")
            .Must(x => x is "reserved")
            .WithMessage("예약 시작모드는 reserved 여야 합니다.")
            .When(x => !string.IsNullOrWhiteSpace(x.시작모드));

        RuleFor(x => x.시작시각)
            .NotNull()
            .WithMessage("예약 시작시각은 필수입니다.")
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("예약 시작시각은 현재보다 미래여야 합니다.")
            .When(x => x.시작시각.HasValue);

        RuleFor(x => x.시작위치)
            .NotEmpty()
            .WithMessage("시작위치는 필수입니다.")
            .MaximumLength(200)
            .WithMessage("시작위치는 200자 이하여야 합니다.")
            .When(x => !string.IsNullOrWhiteSpace(x.시작위치));

        RuleFor(x => x.복귀지)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.복귀지))
            .WithMessage("복귀지는 200자 이하여야 합니다.");
    }
}
