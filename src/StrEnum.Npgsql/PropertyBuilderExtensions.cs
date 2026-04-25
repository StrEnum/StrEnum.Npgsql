using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StrEnum.Npgsql;

public static class PropertyBuilderExtensions
{
    /// <summary>
    /// Configures a <see cref="StringEnum{TEnum}"/> property to be stored as a Postgres enum column.
    /// Sets the column type to the Postgres enum name and applies a value converter that maps the
    /// string enum to its underlying string value.
    /// </summary>
    /// <typeparam name="TEnum">The string enum type.</typeparam>
    /// <param name="propertyBuilder">The property builder.</param>
    /// <param name="name">The Postgres enum type name. Defaults to the snake_cased CLR type name.</param>
    /// <param name="schema">The schema in which the enum lives. Defaults to the model's default schema.</param>
    public static PropertyBuilder<TEnum> HasPostgresStringEnum<TEnum>(this PropertyBuilder<TEnum> propertyBuilder, string? name = null, string? schema = null)
        where TEnum : StringEnum<TEnum>, new()
    {
        if (propertyBuilder == null)
            throw new ArgumentNullException(nameof(propertyBuilder));

        var enumName = name ?? PostgresNaming.ToSnakeCase(typeof(TEnum).Name);
        var columnType = schema is null ? enumName : $"{schema}.{enumName}";

        propertyBuilder.HasConversion(new StringEnumPropertyValueConverter<TEnum>());
        propertyBuilder.HasColumnType(columnType);

        return propertyBuilder;
    }
}
