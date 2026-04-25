using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace StrEnum.Npgsql;

public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Allows Entity Framework to handle string enums when using Npgsql by storing them as <c>text</c>.
    /// This is the default storage mode.
    /// </summary>
    /// <remarks>
    /// To store string enums as Postgres enum types instead, configure the affected properties via
    /// <see cref="ModelBuilderExtensions.MapStringEnumAsPostgresEnum{TEnum}(ModelBuilder, string?, string?)"/>
    /// or <see cref="PropertyBuilderExtensions.HasPostgresStringEnum{TEnum}"/>. <c>UseStringEnums()</c>
    /// is still required in that case so EF Core treats <see cref="StringEnum{TEnum}"/> properties
    /// as scalars rather than navigations.
    /// </remarks>
    public static DbContextOptionsBuilder UseStringEnums(this DbContextOptionsBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        return builder.ReplaceService<IValueConverterSelector, ChainedValueConverterSelectorDecorator>();
    }

    /// <summary>
    /// Allows Entity Framework to handle string enums when using Npgsql by storing them as <c>text</c>.
    /// </summary>
    public static DbContextOptionsBuilder<TContext> UseStringEnums<TContext>(this DbContextOptionsBuilder<TContext> builder)
        where TContext : DbContext
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        UseStringEnums((DbContextOptionsBuilder)builder);

        return builder;
    }
}
