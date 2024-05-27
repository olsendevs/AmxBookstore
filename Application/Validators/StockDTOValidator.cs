using AmxBookstore.Application.DTOs;
using FluentValidation;

namespace AmxBookstore.Application.Validators
{
    public class StockDTOValidator : AbstractValidator<StockDTO>
    {
        public StockDTOValidator()
        {
            RuleFor(x => x.BookId)
                .NotEmpty().WithMessage("Book ID is required.");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity must be zero or greater.");
        }
    }
}
