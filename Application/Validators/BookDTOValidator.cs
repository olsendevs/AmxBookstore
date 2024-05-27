using AmxBookstore.Application.DTOs;
using FluentValidation;

namespace AmxBookstore.Application.Validators
{
    public class BookDTOValidator : AbstractValidator<BookDTO>
    {
        public BookDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");

            RuleFor(x => x.Pages)
                .GreaterThan(0).WithMessage("Pages must be greater than zero.");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author is required.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");
        }
    }
}
