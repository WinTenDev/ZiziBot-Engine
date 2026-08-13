using FluentValidation;

namespace ZiziBot.Application.Services.Validators;

public static class ValidatorExtension
{
    public static IRuleBuilderOptions<T, TProperty> Required<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder) where TProperty : IComparable<TProperty>, IComparable
    {
        var options = ruleBuilder.NotNull();

        // Add NotEmpty for string, collections, and enumerable types
        if (typeof(TProperty) == typeof(string))
        {
            options = options.NotEmpty();
        }
        else if (typeof(IEnumerable<>).IsAssignableFrom(typeof(TProperty)))
        {
            options = options.Must(x => x != null && ((IEnumerable<object>)x).Any());
        }

        // Add greater than 0 check for numeric types
        if (typeof(TProperty).IsNumericType())
        {
            options = options.GreaterThan(default(TProperty));
        }

        return options;
    }

    // Extension method to check if a type is a numeric type
    private static bool IsNumericType(this Type type)
    {
        return type == typeof(int) ||
               type == typeof(long) ||
               type == typeof(short) ||
               type == typeof(float) ||
               type == typeof(double) ||
               type == typeof(decimal) ||
               type == typeof(int?) ||
               type == typeof(long?) ||
               type == typeof(short?) ||
               type == typeof(float?) ||
               type == typeof(double?) ||
               type == typeof(decimal?);
    }
}