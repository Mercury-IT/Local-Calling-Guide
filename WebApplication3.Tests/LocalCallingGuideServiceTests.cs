using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using WebApplication3.Models;
using WebApplication3.Services;

namespace WebApplication3.Tests;

public sealed class LocalCallingGuideServiceTests
{
    [Fact]
    public async Task LookupAsync_DeserializesRateCenterAndLooksUpDuplicatePrefixOnce()
    {
        var requestCount = 0;
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            Interlocked.Increment(ref requestCount);
            Assert.Equal("/xmlprefix.php?npa=212&nxx=555", request.RequestUri!.PathAndQuery);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "<root><inputdata><npa>212</npa></inputdata><prefixdata><npa>212</npa><nxx>555</nxx><rc>New York</rc></prefixdata></root>")
            });
        });
        var service = CreateService(handler);

        var result = await service.LookupAsync(
            [new PhonePrefix("212", "555"), new PhonePrefix("212", "555")],
            CancellationToken.None);

        Assert.Equal(1, requestCount);
        Assert.Equal("New York", result["212-555"].RateCenter);
        Assert.Null(result["212-555"].ErrorMessage);
    }

    [Fact]
    public async Task LookupAsync_WhenRemoteReturnsBadXml_ProvidesVisibleFailure()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<not-closed>")
            }));
        var service = CreateService(handler);

        var result = await service.LookupAsync(
            [new PhonePrefix("212", "555")],
            CancellationToken.None);

        Assert.Null(result["212-555"].RateCenter);
        Assert.Equal("The lookup service is temporarily unavailable. Please try again.", result["212-555"].ErrorMessage);
    }

    [Fact]
    public async Task LookupAsync_WhenPrefixHasNoRateCenter_ReturnsNoMatchMessage()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<root><prefixdata><rc /></prefixdata></root>")
            }));
        var service = CreateService(handler);

        var result = await service.LookupAsync(
            [new PhonePrefix("212", "555")],
            CancellationToken.None);

        Assert.Equal("No rate center was found for this prefix.", result["212-555"].ErrorMessage);
    }

    private static LocalCallingGuideService CreateService(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localcallingguide.test/")
        };
        return new LocalCallingGuideService(client, NullLogger<LocalCallingGuideService>.Instance);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => sendAsync(request, cancellationToken);
    }
}
