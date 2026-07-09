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

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.사유) || !string.IsNullOrWhiteSpace(x.예외코드))
            .WithMessage("문제 사유 또는 예외 코드는 필수입니다.");

        RuleFor(x => x.단계)
            .MaximumLength(50)
            .WithMessage("문제 단계는 50자 이하여야 합니다.")
            .When(x => !string.IsNullOrWhiteSpace(x.단계));

        RuleFor(x => x.예외코드)
            .MaximumLength(80)
            .WithMessage("예외 코드는 80자 이하여야 합니다.")
            .When(x => !string.IsNullOrWhiteSpace(x.예외코드));

        RuleFor(x => x.사유)
            .MaximumLength(500)
            .WithMessage("문제 사유는 500자 이하여야 합니다.")
            .When(x => !string.IsNullOrWhiteSpace(x.사유));

        RuleFor(x => x.메모)
            .MaximumLength(500)
            .WithMessage("메모는 500자 이하여야 합니다.")
            .When(x => !string.IsNullOrWhiteSpace(x.메모));

        RuleFor(x => x.증빙ObjectName)
            .MaximumLength(500)
            .WithMessage("증빙 파일 식별자는 500자 이하여야 합니다.")
            .When(x => !string.IsNullOrWhiteSpace(x.증빙ObjectName));

        RuleFor(x => x.증빙Url)
            .MaximumLength(2000)
            .WithMessage("증빙 URL은 2000자 이하여야 합니다.")
            .When(x => !string.IsNullOrWhiteSpace(x.증빙Url));
    }
}
