using EMS_Application.DTO.Department;
using FluentValidation;

namespace EMS_Application.Validators;

public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(2, 100).WithMessage("Name must be between 2 and 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .Length(2, 10).WithMessage("Code must be between 2 and 10 characters.")
            .Matches(@"^[A-Z0-9]+$").WithMessage("Code must contain only uppercase letters and digits.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}
