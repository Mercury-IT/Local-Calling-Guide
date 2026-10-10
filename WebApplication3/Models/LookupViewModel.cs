using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models;

public sealed class LookupViewModel
{
    [Required(ErrorMessage = "Enter at least one phone number or NPA-NXX.")]
    [StringLength(500000, ErrorMessage = "Enter no more than 500,000 characters.")]
    [Display(Name = "Phone numbers or NPA-NXX prefixes")]
    public string Numbers { get; set; } = string.Empty;

    public IReadOnlyList<RateCenterResult> Results { get; set; } = [];
}

public sealed record PhonePrefix(string Npa, string Nxx)
{
    public string Key => $"{Npa}-{Nxx}";
}

public sealed record ParsedPhoneNumber(string Original, PhonePrefix Prefix);

public sealed record RateCenterResult(string Number, PhonePrefix Prefix, string? RateCenter, string? ErrorMessage)
{
    public bool HasRateCenter => !string.IsNullOrWhiteSpace(RateCenter) && ErrorMessage is null;
}

public sealed record PrefixLookupResult(string? RateCenter, string? ErrorMessage);
