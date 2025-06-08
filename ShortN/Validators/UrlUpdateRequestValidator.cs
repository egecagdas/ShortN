using FluentValidation;
using ShortN.Models;

namespace ShortN.Validators;

public class UrlUpdateRequestValidator : AbstractValidator<UrlUpdateRequest>
{
    public UrlUpdateRequestValidator()
    {
        RuleFor(x => x.LongUrl)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Please provide a valid URL");

        RuleFor(x => x.Ttl)
            .GreaterThan(0)
            .When(x => x.Ttl.HasValue)
            .WithMessage("TTL must be greater than 0");
    }
}