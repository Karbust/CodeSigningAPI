using System.Text.Json;

namespace Infrastructure.Utils;

public static class SerializationHelper
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new BigIntegerConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, Options);
    }

    public static T? Deserialize<T>(ReadOnlySpan<char> source)
    {
        return JsonSerializer.Deserialize<T>(source, Options);
    }
}