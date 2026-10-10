using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models;

public sealed class LookupViewModel
{
    [Required(ErrorMessage = "Enter at least one phone number or NPA-NXX.")]
    [StringLength(500000, ErrorMessage = "Enter no more than 500,000 characters.")]
    [Display(Name = "Phone numbers or NPA-NXX prefixes")]
    public string Numbers { get; set; } = string.Empty;

    public List<RateCenterResult> Results { get; set; } = [];
}

public sealed class PhonePrefix
{
    public string Npa { get; set; } = string.Empty;
    public string Nxx { get; set; } = string.Empty;
    public string Key => $"{Npa}-{Nxx}";

    public PhonePrefix() { }
    public PhonePrefix(string npa, string nxx) { Npa = npa; Nxx = nxx; }
}

public sealed record ParsedPhoneNumber(string Original, PhonePrefix Prefix);

public sealed class RateCenterResult
{
    public string Number { get; set; } = string.Empty;
    public PhonePrefix Prefix { get; set; } = new("", "");
    public string? RateCenter { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasRateCenter => !string.IsNullOrWhiteSpace(RateCenter) && ErrorMessage is null;
}

public sealed record PrefixLookupResult(string? RateCenter, string? ErrorMessage);
