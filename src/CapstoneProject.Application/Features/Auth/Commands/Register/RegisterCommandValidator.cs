using FluentValidation;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Validators;

namespace CapstoneProject.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : BaseAuthValidator<RegisterCommand>
{
    // private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    // private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public RegisterCommandValidator(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        SetupValidationRules();
    }

    protected override void SetupValidationRules()
    {
        RuleFor(x => x.Request.Email)
            .ValidEmailUnique(UnitOfWork);

        RuleFor(x => x.Request.Password)
            .ValidPassword(6);

        RuleFor(x => x.Request.ConfirmPassword)
            .ValidConfirmPassword(x => x.Request.Password);

        RuleFor(x => x.Request.FirstName)
            .ValidPersonName("First name", 50);

        RuleFor(x => x.Request.LastName)
            .ValidPersonName("Last name", 50);

        RuleFor(x => x.Request.PhoneNumber)
            .ValidPhoneNumberUnique(UnitOfWork);

        RuleFor(x => x.Request.Gender)
            .IsInEnum().When(x => x.Request.Gender.HasValue)
            .WithMessage("Invalid gender value");

        RuleFor(x => x.Request.OtpSentChannel)
            .IsInEnum().When(x => x.Request.OtpSentChannel.HasValue)
            .WithMessage("Invalid OTP sent channel");

        // Avatar validation - only when avatar is not null
        // RuleFor(x => x.Request.Avatar)
        //     .Must(BeValidImageFile)
        //     .WithMessage($"Avatar must be an image file ({string.Join(", ", _allowedImageExtensions)}) and less than {MaxFileSizeBytes / (1024 * 1024)}MB")
        //     .When(x => x.Request.Avatar != null);
    }

    // private bool BeValidImageFile(Microsoft.AspNetCore.Http.IFormFile? file)
    // {
    //     if (file == null) return true; // Allow null files

    //     // Check file size
    //     if (file.Length > MaxFileSizeBytes) return false;

    //     // Check file extension
    //     var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
    //     if (!_allowedImageExtensions.Contains(extension)) return false;

    //     // Check content type
    //     var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
    //     if (!allowedContentTypes.Contains(file.ContentType?.ToLowerInvariant())) return false;

    //     return true;
    // }
}