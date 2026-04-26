using System.Text;
using Npgsql.Internal;

namespace StrEnum.Npgsql.Internal;

/// <summary>
/// Reads and writes a <see cref="StringEnum{TEnum}"/> as a Postgres enum value: the wire payload
/// is the underlying string value of the enum member, encoded with the connection's text encoding
/// (UTF-8 by default).
/// </summary>
/// <remarks>
/// Modelled on <c>Npgsql.Internal.Converters.EnumConverter&lt;TEnum&gt;</c>. The notable difference is
/// the type constraint: Npgsql's converter requires <c>where TEnum : struct, Enum</c>, which excludes
/// <see cref="StringEnum{TEnum}"/> (a class). This converter takes the same approach but uses the
/// string-enum's own member registry for label ↔ value lookups instead of reflecting over enum fields.
/// </remarks>
internal sealed class StringEnumConverter<TEnum> : PgBufferedConverter<TEnum>
    where TEnum : StringEnum<TEnum>, new()
{
    private readonly Encoding _encoding;

    public StringEnumConverter(Encoding encoding) => _encoding = encoding;

    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.Value;
        return format is DataFormat.Binary or DataFormat.Text;
    }

    public override Size GetSize(SizeContext context, TEnum value, ref object? writeState)
        => _encoding.GetByteCount((string)value);

    protected override TEnum ReadCore(PgReader reader)
    {
        var label = _encoding.GetString(reader.ReadBytes(reader.CurrentRemaining));

        if (!StringEnum<TEnum>.TryParse(label, out var member, ignoreCase: false, MatchBy.ValueOnly))
            throw new InvalidCastException(
                $"Received enum value '{label}' from database which wasn't found on string enum {typeof(TEnum)}.");

        return member!;
    }

    protected override void WriteCore(PgWriter writer, TEnum value)
    {
        var label = (string)value;
        writer.WriteBytes(new ReadOnlySpan<byte>(_encoding.GetBytes(label)));
    }
}
