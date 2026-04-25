using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace StrEnum.Npgsql;

/// <summary>
/// A value converter that maps a string enum member to its underlying string value, suitable for use
/// with <c>PropertyBuilder.HasConversion</c>. The model CLR type is <typeparamref name="TEnum"/>
/// itself rather than <see cref="StringEnum{TEnum}"/>, which is what EF Core requires when applying
/// a converter to a specific property.
/// </summary>
internal class StringEnumPropertyValueConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : StringEnum<TEnum>, new()
{
    public StringEnumPropertyValueConverter()
        : base(@enum => (string)@enum, value => StringEnum<TEnum>.Parse(value, false, MatchBy.ValueOnly))
    {
    }
}
