using ClosedXML.Excel;
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
                return new RateCenterResult
                {
                    Number = number.Original,
                    Prefix = number.Prefix,
                    RateCenter = result.RateCenter,
                    ErrorMessage = result.ErrorMessage
                };
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Export(LookupViewModel model)
        {
            if (model.Results is null || model.Results.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Rate Centers");

            worksheet.Cell(1, 1).Value = "Phone Number";
            worksheet.Cell(1, 2).Value = "NPA-NXX";
            worksheet.Cell(1, 3).Value = "Rate Center";
            worksheet.Cell(1, 4).Value = "Status";

            var header = worksheet.Range(1, 1, 1, 4);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563eb");
            header.Style.Font.FontColor = XLColor.White;

            for (var i = 0; i < model.Results.Count; i++)
            {
                var item = model.Results[i];
                var row = i + 2;
                worksheet.Cell(row, 1).Value = item.Number;
                worksheet.Cell(row, 2).Value = item.Prefix.Key;
                worksheet.Cell(row, 3).Value = item.HasRateCenter ? item.RateCenter : "";
                worksheet.Cell(row, 4).Value = item.HasRateCenter ? "Found" : (item.ErrorMessage ?? "Not found");
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"RateCenters-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
