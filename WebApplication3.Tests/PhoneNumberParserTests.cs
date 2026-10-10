using WebApplication3.Services;

namespace WebApplication3.Tests;

public sealed class PhoneNumberParserTests
{
    [Theory]
    [InlineData("212-555", "212-555")]
    [InlineData("(212) 555-0199", "212-555")]
    [InlineData("+1 212.555.0199", "212-555")]
    [InlineData("5875970423", "587-597")]
    [InlineData("+15875970423", "587-597")]
    public void TryParse_ValidNumber_ReturnsNpaNxx(string input, string expectedPrefix)
    {
        var parsed = PhoneNumberParser.TryParse(input, out var result);

        Assert.True(parsed);
        Assert.NotNull(result);
        Assert.Equal(expectedPrefix, result.Prefix.Key);
    }

    [Theory]
    [InlineData("")]
    [InlineData("212")]
    [InlineData("112-555-0199")]
    [InlineData("212-155-0199")]
    public void TryParse_InvalidNumber_ReturnsFalse(string input)
    {
        Assert.False(PhoneNumberParser.TryParse(input, out _));
    }

    [Fact]
    public void SplitNumbers_UsesCommonSeparators()
    {
        var entries = PhoneNumberParser.SplitNumbers("212-555\r\n415-555; 617-555, 312-555 | 303-555 / 404-555 \\ 505-555");

        Assert.Equal(
            new[] { "212-555", "415-555", "617-555", "312-555", "303-555", "404-555", "505-555" },
            entries);
    }

    [Fact]
    public void ExceedsMaximumNumbers_RejectsMoreThanFifteenThousand()
    {
        Assert.False(PhoneNumberParser.ExceedsMaximumNumbers(15000));
        Assert.True(PhoneNumberParser.ExceedsMaximumNumbers(15001));
    }
}
