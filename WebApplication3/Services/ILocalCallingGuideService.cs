using WebApplication3.Models;

namespace WebApplication3.Services;

public interface ILocalCallingGuideService
{
    Task<IReadOnlyDictionary<string, PrefixLookupResult>> LookupAsync(
        IReadOnlyCollection<PhonePrefix> prefixes,
        CancellationToken cancellationToken);
}
