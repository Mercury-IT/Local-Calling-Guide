using System.Collections.Concurrent;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using WebApplication3.Models;

namespace WebApplication3.Services;

public sealed class LocalCallingGuideService(
    HttpClient httpClient,
    ILogger<LocalCallingGuideService> logger) : ILocalCallingGuideService
{
    private const int MaximumConcurrentLookups = 5;

    public async Task<IReadOnlyDictionary<string, PrefixLookupResult>> LookupAsync(
        IReadOnlyCollection<PhonePrefix> prefixes,
        CancellationToken cancellationToken)
    {
        var results = new ConcurrentDictionary<string, PrefixLookupResult>(StringComparer.Ordinal);
        var distinctPrefixes = prefixes.DistinctBy(prefix => prefix.Key).ToArray();

        await Parallel.ForEachAsync(
            distinctPrefixes,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = MaximumConcurrentLookups
            },
            async (prefix, token) =>
            {
                results[prefix.Key] = await LookupPrefixAsync(prefix, token);
            });

        return results;
    }

    private async Task<PrefixLookupResult> LookupPrefixAsync(PhonePrefix prefix, CancellationToken cancellationToken)
    {
        try
        {
            var path = $"xmlprefix.php?npa={Uri.EscapeDataString(prefix.Npa)}&nxx={Uri.EscapeDataString(prefix.Nxx)}";
            using var response = await httpClient.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            });
            var document = (LocalCallingGuideResponse?)new XmlSerializer(typeof(LocalCallingGuideResponse))
                .Deserialize(reader);

            var rateCenter = document?.PrefixData?.RateCenter?.Trim();
            return string.IsNullOrWhiteSpace(rateCenter)
                ? new PrefixLookupResult(null, "No rate center was found for this prefix.")
                : new PrefixLookupResult(rateCenter, null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException
            or OperationCanceledException
            or XmlException
            or InvalidOperationException)
        {
            logger.LogWarning(exception, "Rate center lookup failed for prefix {Prefix}.", prefix.Key);
            return new PrefixLookupResult(null, "The lookup service is temporarily unavailable. Please try again.");
        }
    }
}

[XmlRoot("root", Namespace = "", IsNullable = false)]
public sealed class LocalCallingGuideResponse
{
    [XmlElement("prefixdata")]
    public LocalCallingGuidePrefixData? PrefixData { get; set; }
}

public sealed class LocalCallingGuidePrefixData
{
    [XmlElement("rc")]
    public string? RateCenter { get; set; }
}
