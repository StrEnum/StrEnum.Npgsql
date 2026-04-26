using Npgsql;
using Npgsql.TypeMapping;
using StrEnum.Npgsql.Internal;

namespace StrEnum.Npgsql;

public static class NpgsqlDataSourceBuilderExtensions
{
    /// <summary>
    /// Registers a <see cref="StringEnum{TEnum}"/> with the Npgsql data source so parameters of
    /// that CLR type bind to a named Postgres enum on the wire. Mirrors the shape of Npgsql's
    /// built-in <c>MapEnum&lt;TEnum&gt;</c> for regular C# enums.
    /// </summary>
    /// <typeparam name="TEnum">The string enum type.</typeparam>
    /// <param name="builder">The Npgsql data source builder.</param>
    /// <param name="name">The Postgres enum type name. Defaults to the snake_cased CLR type name.</param>
    /// <param name="schema">The schema in which the enum lives. When omitted, Npgsql resolves the type via the connection's <c>search_path</c>.</param>
    public static NpgsqlDataSourceBuilder MapStringEnum<TEnum>(
        this NpgsqlDataSourceBuilder builder,
        string? name = null,
        string? schema = null)
        where TEnum : StringEnum<TEnum>, new()
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        ((INpgsqlTypeMapper)builder).MapStringEnum<TEnum>(name, schema);

        return builder;
    }

    /// <summary>
    /// Registers a <see cref="StringEnum{TEnum}"/> with an Npgsql type mapper. Generic counterpart
    /// that preserves the concrete builder type for fluent chaining (mirrors the
    /// <c>UseNetTopologySuite</c> overload shape on <see cref="INpgsqlTypeMapper"/>).
    /// </summary>
    public static TMapper MapStringEnum<TMapper, TEnum>(
        this TMapper mapper,
        string? name = null,
        string? schema = null)
        where TMapper : INpgsqlTypeMapper
        where TEnum : StringEnum<TEnum>, new()
    {
        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        var pgTypeName = ResolvePgTypeName<TEnum>(name, schema);
        mapper.AddTypeInfoResolverFactory(new StringEnumTypeInfoResolverFactory<TEnum>(pgTypeName));

        return mapper;
    }

    /// <summary>
    /// Registers a <see cref="StringEnum{TEnum}"/> with an <see cref="INpgsqlTypeMapper"/>. Provided
    /// for binary compatibility with <c>NpgsqlConnection.GlobalTypeMapper</c> (legacy global mapping).
    /// </summary>
    public static INpgsqlTypeMapper MapStringEnum<TEnum>(
        this INpgsqlTypeMapper mapper,
        string? name = null,
        string? schema = null)
        where TEnum : StringEnum<TEnum>, new()
    {
        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        var pgTypeName = ResolvePgTypeName<TEnum>(name, schema);
        mapper.AddTypeInfoResolverFactory(new StringEnumTypeInfoResolverFactory<TEnum>(pgTypeName));

        return mapper;
    }

    internal static string ResolvePgTypeName<TEnum>(string? name, string? schema)
        where TEnum : StringEnum<TEnum>, new()
    {
        var enumName = name ?? PostgresNaming.ToSnakeCase(typeof(TEnum).Name);
        return schema is null ? enumName : $"{schema}.{enumName}";
    }
}
