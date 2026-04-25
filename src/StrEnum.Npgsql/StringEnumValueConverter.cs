using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace StrEnum.Npgsql;

/// <summary>
/// Converts between a string enum and its underlying string value. The model CLR type is
/// <see cref="StringEnum{TEnum}"/>, which is what <see cref="StringEnumValueConverterSelector"/>
/// reports to EF Core when it's asked for converters covering all members of the hierarchy.
/// </summary>
internal class StringEnumValueConverter<TEnum> : ValueConverter<StringEnum<TEnum>, string>
    where TEnum : StringEnum<TEnum>, new()
{
    public StringEnumValueConverter(ConverterMappingHints? mappingHints = null)
        : base(@enum => (string)@enum, value => StringEnum<TEnum>.Parse(value, false, MatchBy.ValueOnly), mappingHints)
    {
    }
}
