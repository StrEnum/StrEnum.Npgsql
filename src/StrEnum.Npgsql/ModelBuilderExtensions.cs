using Microsoft.EntityFrameworkCore;

namespace StrEnum.Npgsql;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Registers a Postgres enum type in the EF Core model based on a <see cref="StringEnum{TEnum}"/>.
    /// EF Core will produce a <c>CREATE TYPE ... AS ENUM (...)</c> migration with labels taken from the string enum members.
    /// </summary>
    /// <typeparam name="TEnum">The string enum type.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The Postgres enum type name. Defaults to the snake_cased CLR type name.</param>
    /// <param name="schema">The schema in which to create the enum. Defaults to the model's default schema.</param>
    public static ModelBuilder HasPostgresStringEnum<TEnum>(this ModelBuilder modelBuilder, string? name = null, string? schema = null)
        where TEnum : StringEnum<TEnum>, new()
    {
        if (modelBuilder == null)
            throw new ArgumentNullException(nameof(modelBuilder));

        var labels = StringEnumLabels.For<TEnum>();
        var enumName = name ?? PostgresNaming.ToSnakeCase(typeof(TEnum).Name);

        return modelBuilder.HasPostgresEnum(schema, enumName, labels);
    }

    /// <summary>
    /// Maps a <see cref="StringEnum{TEnum}"/> to a Postgres enum type across the entire model:
    /// registers the enum (creates a <c>CREATE TYPE</c> migration) and configures every property of type
    /// <typeparamref name="TEnum"/> on every entity to use that Postgres enum as its column type.
    /// </summary>
    /// <typeparam name="TEnum">The string enum type.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="name">The Postgres enum type name. Defaults to the snake_cased CLR type name.</param>
    /// <param name="schema">The schema in which to create the enum. Defaults to the model's default schema.</param>
    public static ModelBuilder MapStringEnumAsPostgresEnum<TEnum>(this ModelBuilder modelBuilder, string? name = null, string? schema = null)
        where TEnum : StringEnum<TEnum>, new()
    {
        if (modelBuilder == null)
            throw new ArgumentNullException(nameof(modelBuilder));

        modelBuilder.HasPostgresStringEnum<TEnum>(name, schema);

        var enumName = name ?? PostgresNaming.ToSnakeCase(typeof(TEnum).Name);
        var columnType = schema is null ? enumName : $"{schema}.{enumName}";

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType is null)
                continue;

            var entityBuilder = modelBuilder.Entity(entityType.ClrType);

            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType != typeof(TEnum))
                    continue;

                entityBuilder.Property(property.Name)
                    .HasConversion(new StringEnumPropertyValueConverter<TEnum>())
                    .HasColumnType(columnType);
            }
        }

        return modelBuilder;
    }
}
