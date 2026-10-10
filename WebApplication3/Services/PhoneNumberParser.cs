using System.Text.RegularExpressions;
using WebApplication3.Models;

namespace WebApplication3.Services;

public static partial class PhoneNumberParser
{
    private const int MaximumNumbers = 25;

    public static IReadOnlyList<string> SplitNumbers(string input) =>
        NumberSeparators().Split(input)
            .Select(number => number.Trim())
            .Where(number => number.Length > 0)
            .ToArray();

    public static bool TryParse(string input, out ParsedPhoneNumber? parsed)
    {
        parsed = null;
        if (string.IsNullOrWhiteSpace(input) || !AllowedCharacters().IsMatch(input))
        {
            return false;
        }

        var digits = NonDigits().Replace(input, string.Empty);
        if (digits.Length == 11 && digits[0] == '1')
        {
            digits = digits[1..];
        }

        if (digits.Length is not (6 or 10)
            || digits[0] is < '2' or > '9'
            || digits[3] is < '2' or > '9')
        {
            return false;
        }

        parsed = new ParsedPhoneNumber(input.Trim(), new PhonePrefix(digits[..3], digits.Substring(3, 3)));
        return true;
    }

    public static bool ExceedsMaximumNumbers(int count) => count > MaximumNumbers;

    [GeneratedRegex(@"[\r\n,;|]+")]
    private static partial Regex NumberSeparators();

    [GeneratedRegex(@"^\+?[\d\s().-]+$")]
    private static partial Regex AllowedCharacters();

    [GeneratedRegex(@"[^0-9]")]
    private static partial Regex NonDigits();
}
