using FluentValidation.Results;

namespace EMS_Application.Common;

public static class ValidationExtensions
{
    public static IDictionary<string, string[]> ToErrorDictionary(this ValidationResult result)
    {
        return result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
    }
}
