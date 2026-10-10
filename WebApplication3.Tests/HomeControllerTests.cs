using Microsoft.AspNetCore.Mvc;
using WebApplication3;
using WebApplication3.Models;
using WebApplication3.Services;

namespace WebApplication3.Tests;

public sealed class HomeControllerTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Index_WhenInputIsEmpty_ReturnsValidationErrorWithoutCallingLookup(string input)
    {
        var service = new NeverLookupService();
        var controller = new HomeController(service);

        var result = await controller.Index(
            new LookupViewModel { Numbers = input },
            CancellationToken.None);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(0, service.CallCount);
    }

    private sealed class NeverLookupService : ILocalCallingGuideService
    {
        public int CallCount { get; private set; }

        public Task<IReadOnlyDictionary<string, PrefixLookupResult>> LookupAsync(
            IReadOnlyCollection<PhonePrefix> prefixes,
            CancellationToken cancellationToken)
        {
            CallCount++;
            throw new InvalidOperationException("The lookup service should not be called for empty input.");
        }
    }
}
