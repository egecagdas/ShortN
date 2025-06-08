using FluentValidation;
using ShortN.Models;

namespace ShortN.Validators;

public class UrlRequestValidator : AbstractValidator<UrlRequest>
{
    public UrlRequestValidator()
    {
        RuleFor(x => x.LongUrl)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Please provide a valid URL");

        RuleFor(x => x.CustomCode)
            .Matches(@"^[a-zA-Z0-9-_]{3,20}$")
            .When(x => x.CustomCode != null)
            .WithMessage("Custom code must be 3-20 characters long and can only contain letters, numbers, hyphens, and underscores");

        RuleFor(x => x.Ttl)
            .GreaterThan(0)
            .When(x => x.Ttl.HasValue)
            .WithMessage("TTL must be greater than 0");
    }
}