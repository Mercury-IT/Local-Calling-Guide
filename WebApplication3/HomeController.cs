using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using WebApplication3.Services;

namespace WebApplication3
{
    public class HomeController : Controller
    {
        private const string InvalidNumberMessage =
            "Use a 6-digit NPA-NXX, a 10-digit North American phone number, or an 11-digit number starting with 1.";

        private readonly ILocalCallingGuideService _localCallingGuideService;

        public HomeController(ILocalCallingGuideService localCallingGuideService)
        {
            _localCallingGuideService = localCallingGuideService;
        }

        public IActionResult Index()
        {
            return View(new LookupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LookupViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var entries = PhoneNumberParser.SplitNumbers(model.Numbers ?? string.Empty);
            if (entries.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Enter at least one phone number or NPA-NXX.");
            }

            if (PhoneNumberParser.ExceedsMaximumNumbers(entries.Count))
            {
                ModelState.AddModelError(string.Empty, "Search up to 15,000 numbers at a time.");
                return View(model);
            }

            var parsedNumbers = new List<ParsedPhoneNumber>(entries.Count);
            foreach (var entry in entries)
            {
                if (PhoneNumberParser.TryParse(entry, out var parsed) && parsed is not null)
                {
                    parsedNumbers.Add(parsed);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"“{entry}” is not a valid phone number or NPA-NXX. {InvalidNumberMessage}");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var lookups = await _localCallingGuideService.LookupAsync(
                parsedNumbers.Select(number => number.Prefix).ToArray(),
                cancellationToken);

            model.Results = parsedNumbers.Select(number =>
            {
                var result = lookups[number.Prefix.Key];
                return new RateCenterResult(number.Original, number.Prefix, result.RateCenter, result.ErrorMessage);
            }).ToArray();

            return View(model);
        }
    }
}
