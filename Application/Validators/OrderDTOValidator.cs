using AmxBookstore.Application.DTOs;
using FluentValidation;

namespace AmxBookstore.Application.Validators
{
    public class OrderDTOValidator : AbstractValidator<OrderDTO>
    {
        public OrderDTOValidator()
        {

            RuleForEach(x => x.Products).SetValidator(new OrderItemDTOValidator());

            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("Client ID is required.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(status => new[] { "Created", "Delivering", "Canceled", "Finished" }.Contains(status))
                .WithMessage("Invalid status.");

        }
    }

    public class OrderItemDTOValidator : AbstractValidator<OrderItemDTO>
    {
        public OrderItemDTOValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        }
    }

}
