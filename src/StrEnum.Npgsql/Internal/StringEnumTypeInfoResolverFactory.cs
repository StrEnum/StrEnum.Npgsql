using Npgsql.Internal;
using Npgsql.Internal.Postgres;

namespace StrEnum.Npgsql.Internal;

/// <summary>
/// Wires a <see cref="StringEnum{TEnum}"/> CLR type to a named Postgres enum type at the data source
/// level, so Npgsql sends and receives the value with the right type OID instead of falling back
/// to <c>text</c>.
/// </summary>
/// <remarks>
/// Modelled on <c>Npgsql.NetTopologySuite.Internal.NetTopologySuiteTypeInfoResolverFactory</c>.
/// </remarks>
internal sealed class StringEnumTypeInfoResolverFactory<TEnum> : PgTypeInfoResolverFactory
    where TEnum : StringEnum<TEnum>, new()
{
    private readonly string _pgTypeName;

    public StringEnumTypeInfoResolverFactory(string pgTypeName) => _pgTypeName = pgTypeName;

    public override IPgTypeInfoResolver CreateResolver() => new Resolver(_pgTypeName);

    public override IPgTypeInfoResolver? CreateArrayResolver() => new ArrayResolver(_pgTypeName);

    private class Resolver : IPgTypeInfoResolver
    {
        protected readonly string PgTypeName;

        private TypeInfoMappingCollection? _mappings;
        protected TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new TypeInfoMappingCollection(), PgTypeName);

        public Resolver(string pgTypeName) => PgTypeName = pgTypeName;

        public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        protected static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings, string pgTypeName)
        {
            mappings.AddType<TEnum>(
                pgTypeName,
                (options, mapping, _) => mapping.CreateInfo(
                    options,
                    new StringEnumConverter<TEnum>(options.TextEncoding),
                    preferredFormat: DataFormat.Text),
                isDefault: true);

            return mappings;
        }
    }

    private sealed class ArrayResolver : Resolver, IPgTypeInfoResolver
    {
        private TypeInfoMappingCollection? _arrayMappings;
        private new TypeInfoMappingCollection Mappings => _arrayMappings ??= AddArrayMappings(new TypeInfoMappingCollection(base.Mappings), PgTypeName);

        public ArrayResolver(string pgTypeName) : base(pgTypeName) { }

        public new PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        private static TypeInfoMappingCollection AddArrayMappings(TypeInfoMappingCollection mappings, string pgTypeName)
        {
            mappings.AddArrayType<TEnum>(pgTypeName);
            return mappings;
        }
    }
}
