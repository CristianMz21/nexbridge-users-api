using Nexbridge.UsersApi.Application.Validation;

namespace Nexbridge.UsersApi.Tests.Unit;

public class UserValidatorTests
{
    [Fact]
    public void Validate_WithValidValues_ReturnsNoErrors()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john@example.com";
        const int age = 32;

        // Act
        var errors = UserValidator.Validate(firstName, lastName, email, age);

        // Assert
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("", "Doe", "john@example.com", 30, "firstName")]
    [InlineData("John", "", "john@example.com", 30, "lastName")]
    [InlineData("John", "Doe", "", 30, "email")]
    [InlineData("John", "Doe", "john@example.com", 0, "age")]
    public void Validate_WithInvalidValue_ReturnsExpectedFieldError(
        string firstName,
        string lastName,
        string email,
        int age,
        string expectedErrorField
    )
    {
        // Arrange + Act
        var errors = UserValidator.Validate(firstName, lastName, email, age);

        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(expectedErrorField, errors.Keys);
    }

    [Fact]
    public void NormalizeName_And_NormalizeEmail_TrimsAndLowercasesValues()
    {
        // Arrange
        const string name = "  john  ";
        const string email = "  JOHN@EXAMPLE.COM  ";

        // Act
        var normalizedName = UserValidator.NormalizeName(name);
        var normalizedEmail = UserValidator.NormalizeEmail(email);

        // Assert
        Assert.Equal("john", normalizedName);
        Assert.Equal("john@example.com", normalizedEmail);
    }
}
