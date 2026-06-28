using ApolloSpoilers.Application.DTOs;
using FluentValidation;

namespace ApolloSpoilers.Application.Validation;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Price).GreaterThan(0).LessThan(1_000_000);
        RuleFor(x => x.CompareAtPrice).GreaterThan(0).When(x => x.CompareAtPrice.HasValue);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Material).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Material));
        RuleFor(x => x.Color).MaximumLength(60).When(x => !string.IsNullOrWhiteSpace(x.Color));
        RuleFor(x => x.CarBrand).MaximumLength(80).When(x => !string.IsNullOrWhiteSpace(x.CarBrand));
        RuleFor(x => x.CarModel).MaximumLength(80).When(x => !string.IsNullOrWhiteSpace(x.CarModel));
        RuleFor(x => x.InitialStock).GreaterThanOrEqualTo(0).When(x => x.InitialStock != 0);
        RuleFor(x => x.LowStockThreshold).InclusiveBetween(0, 1000);
        RuleFor(x => x.FitYearFrom).InclusiveBetween(1950, 2100).When(x => x.FitYearFrom.HasValue);
        RuleFor(x => x.FitYearTo).InclusiveBetween(1950, 2100).When(x => x.FitYearTo.HasValue);
    }
}

public class AddToCartValidator : AbstractValidator<AddToCartDto>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(100);
    }
}

public class UpdateCartItemValidator : AbstractValidator<UpdateCartItemDto>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(100);
    }
}

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ShippingFullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.ShippingAddressLine).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ShippingCity).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ShippingState).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ShippingPostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ShippingCountry).NotEmpty().MaximumLength(80);
        RuleFor(x => x.ShippingPhone).MaximumLength(30).When(x => !string.IsNullOrWhiteSpace(x.ShippingPhone));
    }
}

public class SendMessageValidator : AbstractValidator<SendMessageDto>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}
