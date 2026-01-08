using FluentValidation;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Validators;

namespace CapstoneProject.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommandValidator : BaseAuthValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        SetupValidationRules();
    }

    protected override void SetupValidationRules()
    {
        // Validate contact format based on channel only, no database check
        // because user data might be cached and not yet saved to database
        RuleFor(x => x.Request.Contact)
            .ValidContactByChannelOnly(x => x.Request.OtpSentChannel);

        RuleFor(x => x.Request.Otp)
            .ValidOtp();

        RuleFor(x => x.Request.OtpSentChannel)
            .IsInEnum()
            .WithMessage("OTP sent channel is required");

        RuleFor(x => x.Request.OtpType)
            .IsInEnum()
            .WithMessage("OTP type is required");
    }
}
