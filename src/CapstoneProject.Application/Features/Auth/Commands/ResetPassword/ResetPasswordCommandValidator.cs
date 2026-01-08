using FluentValidation;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Validators;

namespace CapstoneProject.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandValidator : BaseAuthValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        SetupValidationRules();
    }

    protected override void SetupValidationRules()
    {
        RuleFor(x => x.Request.Contact)
            .ValidContactByChannel(x => x.Request.OtpSentChannel);

        RuleFor(x => x.Request.NewPassword)
            .ValidPassword(8);

        RuleFor(x => x.Request.OtpSentChannel)
            .IsInEnum()
            .WithMessage("OTP sent channel is required");
    }
}