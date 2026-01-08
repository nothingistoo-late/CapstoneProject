using FluentValidation;
using CapstoneProject.Application.Common.Interfaces;

namespace CapstoneProject.Application.Common.Validators;

/// <summary>
/// Base validator class for authentication-related commands
/// Provides common validation logic and dependency injection
/// </summary>
/// <typeparam name="T">The command type to validate</typeparam>
public abstract class BaseAuthValidator<T> : AbstractValidator<T>
{
    protected readonly IUnitOfWork UnitOfWork;

    protected BaseAuthValidator(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
    }

    /// <summary>
    /// Common validation rules that can be applied to any authentication command
    /// Override this method in derived classes to add specific validation rules
    /// </summary>
    protected virtual void SetupValidationRules()
    {
        // Base implementation - override in derived classes
    }
}
