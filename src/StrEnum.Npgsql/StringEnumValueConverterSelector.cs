using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace StrEnum.Npgsql;

/// <summary>
/// Lets Entity Framework pick a value converter when it encounters a string enum property,
/// so the property is recognised as a scalar instead of a navigation.
/// </summary>
internal class StringEnumValueConverterSelector : IValueConverterSelector
{
    private readonly ConcurrentDictionary<Type, ValueConverterInfo> _converters = new();

    public IEnumerable<ValueConverterInfo> Select(Type modelClrType, Type? providerClrType = null)
    {
        var underlyingModelType = UnwrapNullableType(modelClrType);
        var underlyingProviderType = providerClrType != null ? UnwrapNullableType(providerClrType) : null;

        if (underlyingProviderType is null || underlyingProviderType == typeof(string))
        {
            if (_converters.TryGetValue(underlyingModelType, out var cachedConverter))
                return new[] { cachedConverter };

            if (underlyingModelType.IsStringEnum())
            {
                var converter = _converters.GetOrAdd(underlyingModelType, BuildConverterInfo);
                return new[] { converter };
            }
        }

        return Array.Empty<ValueConverterInfo>();
    }

    private static ValueConverterInfo BuildConverterInfo(Type stringEnum)
    {
        var converterType = typeof(StringEnumValueConverter<>).MakeGenericType(stringEnum);
        var converter = Activator.CreateInstance(converterType, (ConverterMappingHints?)null) as ValueConverter;

        return new ValueConverterInfo(stringEnum, typeof(string), _ => converter!, null);
    }

    private static Type UnwrapNullableType(Type type) => Nullable.GetUnderlyingType(type) ?? type;
}
