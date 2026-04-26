# StrEnum.Npgsql

Lets you map [StrEnum](https://github.com/StrEnum/StrEnum/) string enums to native Postgres enum types via the [Npgsql](https://www.npgsql.org/) ADO.NET driver — analogous to what [`MapEnum`](https://www.npgsql.org/doc/types/enums_and_composites.html) does for regular C# enums.

For Entity Framework Core integration, install [StrEnum.Npgsql.EntityFrameworkCore](https://github.com/StrEnum/StrEnum.Npgsql.EntityFrameworkCore/), which adds model-level registration and migrations on top of this package.

Supports Npgsql 8 – 10. Targets net8.0, net9.0, net10.0.

## Installation

Install [StrEnum.Npgsql](https://www.nuget.org/packages/StrEnum.Npgsql/) via the .NET CLI:

```
dotnet add package StrEnum.Npgsql
```

## Usage

### Defining a string enum

```csharp
public class Sport : StringEnum<Sport>
{
    public static readonly Sport RoadCycling = Define("ROAD_CYCLING");
    public static readonly Sport MountainBiking = Define("MTB");
    public static readonly Sport TrailRunning = Define("TRAIL_RUNNING");
}
```

### Registering with the data source

Assuming the Postgres enum type already exists in the database (`CREATE TYPE sport AS ENUM ('ROAD_CYCLING', 'MTB', 'TRAIL_RUNNING')`), call `MapStringEnum<TEnum>()` on the data source builder to teach Npgsql how to bind it on the wire:

```csharp
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

dataSourceBuilder.MapStringEnum<Sport>();                         // public.sport
dataSourceBuilder.MapStringEnum<Sport>("sport_kind", "races");   // races.sport_kind

await using var dataSource = dataSourceBuilder.Build();
```

`MapStringEnum<TEnum>()` mirrors the shape of Npgsql's built-in [`MapEnum<TEnum>()`](https://www.npgsql.org/doc/types/enums_and_composites.html#enums) for regular C# enums, and registers a [`PgTypeInfoResolverFactory`](https://github.com/npgsql/npgsql/tree/main/src/Npgsql.NetTopologySuite/Internal) that maps `Sport` ↔ the named Postgres enum's OID.

### Using it

Bind a `Sport` instance to a parameter — Npgsql sends it as the enum type:

```csharp
await using var insert = dataSource.CreateCommand();
insert.CommandText = "INSERT INTO races (id, name, sport) VALUES ($1, $2, $3)";
insert.Parameters.AddWithValue(Guid.NewGuid());
insert.Parameters.AddWithValue("Cape Epic");
insert.Parameters.AddWithValue(Sport.MountainBiking);
await insert.ExecuteNonQueryAsync();
```

Read it back the same way — values come out as `Sport` instances:

```csharp
await using var select = dataSource.CreateCommand();
select.CommandText = "SELECT sport FROM races WHERE id = $1";
select.Parameters.AddWithValue(raceId);

var sport = (Sport)(await select.ExecuteScalarAsync())!;
```

`MapStringEnum<TEnum>()` is also available on `INpgsqlTypeMapper` (for use with `NpgsqlConnection.GlobalTypeMapper` or other type-mapper implementations) and on the slim data source builder — same overload shape as Npgsql's `MapEnum<TEnum>`.

## Acknowledgements

The wire-level type-info resolver is modelled directly on [`Npgsql.NetTopologySuite`](https://github.com/npgsql/npgsql/tree/main/src/Npgsql.NetTopologySuite) and [`Npgsql.Internal.Converters.EnumConverter`](https://github.com/npgsql/npgsql/blob/main/src/Npgsql/Internal/Converters/EnumConverter.cs).

## License

Copyright &copy; 2026 [Dmytro Khmara](https://dmytrokhmara.com).

StrEnum is licensed under the [MIT license](LICENSE.txt).
