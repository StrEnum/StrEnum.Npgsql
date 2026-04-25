using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace StrEnum.Npgsql;

/// <summary>
/// Combines value converters from EF Core's default <see cref="ValueConverterSelector"/> with
/// the ones produced for string enums by <see cref="StringEnumValueConverterSelector"/>.
/// </summary>
internal class ChainedValueConverterSelectorDecorator : IValueConverterSelector
{
    private readonly ValueConverterSelector _defaultSelector;
    private readonly StringEnumValueConverterSelector _stringEnumSelector;

    public ChainedValueConverterSelectorDecorator(ValueConverterSelectorDependencies defaultSelectorDependencies)
    {
        _defaultSelector = new ValueConverterSelector(defaultSelectorDependencies);
        _stringEnumSelector = new StringEnumValueConverterSelector();
    }

    public IEnumerable<ValueConverterInfo> Select(Type modelClrType, Type? providerClrType = null)
    {
        var defaultConverters = _defaultSelector.Select(modelClrType, providerClrType);
        var stringEnumConverters = _stringEnumSelector.Select(modelClrType, providerClrType);

        foreach (var converterInfo in defaultConverters.Concat(stringEnumConverters))
            yield return converterInfo;
    }
}
