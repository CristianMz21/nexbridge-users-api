using System.Net.Mail;

namespace Nexbridge.UsersApi.Application.Validation;

public static class UserValidator
{
    private const int MaximumNameLength = 100;
    private const int MaximumEmailLength = 254;
    private const int MinimumAge = 1;
    private const int MaximumAge = 120;

    public static Dictionary<string, string[]> Validate(
        string? firstName,
        string? lastName,
        string? email,
        int age
    )
    {
        var errors = new Dictionary<string, string[]>();

        ValidateName(firstName, "firstName", "First name", errors);
        ValidateName(lastName, "lastName", "Last name", errors);
        ValidateEmail(email, errors);
        ValidateAge(age, errors);

        return errors;
    }

    public static string NormalizeName(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    public static string NormalizeEmail(string? value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static void ValidateName(string? value, string fieldName, string displayName, IDictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            AddError(errors, fieldName, $"{displayName} is required.");
            return;
        }

        if (value.Length > MaximumNameLength)
        {
            AddError(errors, fieldName, $"{displayName} cannot exceed {MaximumNameLength} characters.");
        }
    }

    private static void ValidateEmail(string? email, IDictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            AddError(errors, "email", "Email is required.");
            return;
        }

        var normalizedEmail = email.Trim();

        if (normalizedEmail.Length > MaximumEmailLength)
        {
            AddError(errors, "email", $"Email cannot exceed {MaximumEmailLength} characters.");
            return;
        }

        if (
            !MailAddress.TryCreate(normalizedEmail, out var parsedEmail)
            || !string.Equals(parsedEmail.Address, normalizedEmail, StringComparison.OrdinalIgnoreCase)
        )
        {
            AddError(errors, "email", "Email format is invalid.");
        }
    }

    private static void ValidateAge(int age, IDictionary<string, string[]> errors)
    {
        if (age is < MinimumAge or > MaximumAge)
        {
            AddError(errors, "age", $"Age must be between {MinimumAge} and {MaximumAge}.");
        }
    }

    private static void AddError(IDictionary<string, string[]> errors, string fieldName, string error)
    {
        errors[fieldName] = [error];
    }
}
