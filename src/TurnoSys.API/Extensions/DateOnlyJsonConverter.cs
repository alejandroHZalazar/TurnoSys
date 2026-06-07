using System.Text.Json;
using System.Text.Json.Serialization;

namespace TurnoSys.API.Extensions;

/// <summary>
/// Conversor System.Text.Json para DateOnly (formato ISO 8601: "yyyy-MM-dd").
/// Necesario porque System.Text.Json no soporta DateOnly de forma nativa hasta .NET 7+
/// pero el binding de controllers sí lo requiere explícito.
/// </summary>
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value)) return default;
        return DateOnly.ParseExact(value, Format);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format));
}

// Conversor para DateOnly? (nullable)
public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value)) return null;
        return DateOnly.ParseExact(value, Format);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else writer.WriteStringValue(value.Value.ToString(Format));
    }
}
