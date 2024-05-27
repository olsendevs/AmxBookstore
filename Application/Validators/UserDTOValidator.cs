using AmxBookstore.Application.DTOs;
using FluentValidation;

namespace AmxBookstore.Application.Validators
{
    public class UserDTOValidator : AbstractValidator<UserDTO>
    {
        public UserDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters long.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(role => new[] { "Admin", "Seller", "Client" }.Contains(role))
                .WithMessage("Invalid role.");
        }
    }
}
