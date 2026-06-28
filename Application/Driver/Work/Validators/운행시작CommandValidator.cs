using FluentValidation;
using Hongdal.Contracts.Driver.Work;

namespace Hongdal.Application.Driver.Work;

public sealed class 운행시작CommandValidator : AbstractValidator<운행시작Command>
{
    public 운행시작CommandValidator()
    {
        RuleFor(x => x.기사Id)
            .NotEmpty()
            .WithMessage("기사 인증 정보가 없습니다.");

        RuleFor(x => x.시작모드)
            .NotEmpty()
            .WithMessage("시작모드는 필수입니다.")
            .Must(x => x is "바로시작" or "예약대기" or "복귀콜찾기")
            .WithMessage("지원하지 않는 시작모드입니다.")
            .When(x => !string.IsNullOrWhiteSpace(x.시작모드));

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
