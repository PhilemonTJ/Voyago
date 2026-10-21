using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Voyago.BookingService.Data;

internal static class ConcurrencyToken
{
    public static bool TryReadIfMatch(string? value, out uint version)
    {
        version = default;

        return value is { Length: >= 3 } &&
            value[0] == '"' &&
            value[^1] == '"' &&
            uint.TryParse(
                value.AsSpan(1, value.Length - 2),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out version);
    }

    public static void SetEtag(HttpResponse response, uint version)
    {
        response.Headers["ETag"] = $"\"{version}\"";
    }

    public static void ApplyExpectedVersion(
        DbContext db,
        object entity,
        uint expectedVersion)
    {
        var token = db.Entry(entity).Property("xmin");
        if (token.CurrentValue is not uint currentVersion ||
            currentVersion != expectedVersion)
        {
            throw new DbUpdateConcurrencyException(
                "The resource has changed since it was read.");
        }

        token.OriginalValue = expectedVersion;
    }
}
