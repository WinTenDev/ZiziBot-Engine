using FluentValidation;
using FluentValidation.Results;

namespace ZiziBot.Application.Common.Utils;

public static class FValidationUtil
{
    public static ValidationResult? Validate<TValidator, TObj>(this TObj toValidate)
        where TValidator : class, new() where TObj : class
    {
        var validatorInstance = new TValidator() as AbstractValidator<TObj>;
        var results = validatorInstance?.Validate(toValidate);

        return results;
    }

    public static async Task<ValidationResult> ValidateAsync<TValidator, TObj>(this TObj toValidate)
        where TValidator : class, new() where TObj : class
    {
        var validatorInstance = new TValidator() as AbstractValidator<TObj>;
        var results = await validatorInstance?.ValidateAsync(toValidate)!;

        return results;
    }
}