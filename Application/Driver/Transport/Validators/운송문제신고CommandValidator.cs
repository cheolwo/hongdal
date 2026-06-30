using FluentValidation;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송문제신고CommandValidator : AbstractValidator<운송문제신고Command>
{
    public 운송문제신고CommandValidator()
    {
        RuleFor(x => x.기사Id)
            .NotEmpty()
            .WithMessage("기사 인증 정보가 없습니다.");

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("운송 Id가 올바르지 않습니다.");

        RuleFor(x => x.사유)
            .NotEmpty()
            .WithMessage("문제 사유는 필수입니다.");

        RuleFor(x => x.사유)
            .MaximumLength(500)
            .WithMessage("문제 사유는 500자 이하여야 합니다.")
            .When(x => !string.IsNullOrWhiteSpace(x.사유));

        RuleFor(x => x.메모)
            .MaximumLength(500)
            .WithMessage("메모는 500자 이하여야 합니다.")
            .When(x => !string.IsNullOrWhiteSpace(x.메모));
    }
}
