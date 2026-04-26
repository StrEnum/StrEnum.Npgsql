using FluentAssertions;
using Npgsql;
using Xunit;

namespace StrEnum.Npgsql.IntegrationTests;

/// <summary>
/// Exercises the wire-level <c>NpgsqlDataSourceBuilder.MapStringEnum&lt;T&gt;</c> registration without
/// EF Core in the loop — proves the resolver/converter pair (modelled on
/// <c>NpgsqlNetTopologySuiteExtensions</c>) binds <see cref="StringEnum{TEnum}"/> instances to a
/// native Postgres enum on the wire end-to-end.
/// </summary>
public class RawNpgsqlRoundTripTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _postgres;

    public RawNpgsqlRoundTripTests(PostgresFixture postgres) => _postgres = postgres;

    [Fact]
    public async Task Round_trips_a_string_enum_against_a_native_postgres_enum_column()
    {
        // The PG type and table need to exist before the mapped data source connects, because
        // Npgsql caches the database type catalog at first connection and the resolver looks the
        // enum's OID up from that cache.
        await using (var setupConnection = new NpgsqlConnection(_postgres.ConnectionString))
        {
            await setupConnection.OpenAsync();
            await using var ddl = setupConnection.CreateCommand();
            ddl.CommandText =
                "DROP TABLE IF EXISTS races_raw;\n" +
                "DROP TYPE IF EXISTS sport;\n" +
                "CREATE TYPE sport AS ENUM ('ROAD_CYCLING', 'MTB', 'TRAIL_RUNNING');\n" +
                "CREATE TABLE races_raw (id uuid PRIMARY KEY, name text NOT NULL, sport sport NOT NULL);";
            await ddl.ExecuteNonQueryAsync();
        }

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_postgres.ConnectionString);
        dataSourceBuilder.MapStringEnum<Sport>();

        await using var dataSource = dataSourceBuilder.Build();

        await using (var insert = dataSource.CreateCommand())
        {
            insert.CommandText = "INSERT INTO races_raw (id, name, sport) VALUES ($1, $2, $3)";
            insert.Parameters.AddWithValue(Guid.NewGuid());
            insert.Parameters.AddWithValue("Chornohora Sky Marathon");
            insert.Parameters.AddWithValue(Sport.TrailRunning);
            await insert.ExecuteNonQueryAsync();
        }

        await using var select = dataSource.CreateCommand();
        select.CommandText = "SELECT name FROM races_raw WHERE sport = $1";
        select.Parameters.AddWithValue(Sport.TrailRunning);

        var name = (string?)await select.ExecuteScalarAsync();

        name.Should().Be("Chornohora Sky Marathon");
    }
}
