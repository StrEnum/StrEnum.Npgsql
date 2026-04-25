# StrEnum.Npgsql

Lets you use [StrEnum](https://github.com/StrEnum/StrEnum/) string enums with [Npgsql](https://www.npgsql.org/) and Entity Framework Core, including the ability to map them to native Postgres enum types — similar to what [`MapEnum`](https://www.npgsql.org/efcore/mapping/enum.html) does for regular C# enums.

Supports EF Core 6 – 10.

## Installation

Install [StrEnum.Npgsql](https://www.nuget.org/packages/StrEnum.Npgsql/) via the .NET CLI:

```
dotnet add package StrEnum.Npgsql
```

## Usage

`StrEnum.Npgsql` lets you choose how Entity Framework stores your string enums in Postgres:

* as plain **text** columns (the default), or
* as native **Postgres enum** types created via `CREATE TYPE ... AS ENUM (...)`.

### Storing string enums as text

#### Defining a string enum and an entity

```csharp
public class Sport: StringEnum<Sport>
{
    public static readonly Sport RoadCycling = Define("ROAD_CYCLING");
    public static readonly Sport MountainBiking = Define("MTB");
    public static readonly Sport TrailRunning = Define("TRAIL_RUNNING");
}

public class Race
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Sport Sport { get; private set; }

    private Race() { }

    public Race(string name, Sport sport)
    {
        Id = Guid.NewGuid();
        Name = name;
        Sport = sport;
    }
}
```

#### Wiring it up

Call `UseStringEnums()` when configuring your DB context:

```csharp
public class RaceContext: DbContext
{
    public DbSet<Race> Races { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql("Host=localhost;Database=BestRaces;Username=*;Password=*;")
            .UseStringEnums();
    }
}
```

EF Core stores the `Sport` property in a `text` column. Running `dotnet ef migrations add Init` produces:

```csharp
migrationBuilder.CreateTable(
    name: "Races",
    columns: table => new
    {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        Name = table.Column<string>(type: "text", nullable: false),
        Sport = table.Column<string>(type: "text", nullable: false)
    },
    constraints: table => table.PrimaryKey("PK_Races", x => x.Id));
```

### Storing string enums as Postgres enum types

To map `Sport` to a Postgres enum type called `sport`, keep `UseStringEnums()` on the options builder and call `MapStringEnumAsPostgresEnum<Sport>()` in `OnModelCreating`:

```csharp
public class RaceContext: DbContext
{
    public DbSet<Race> Races { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql("Host=localhost;Database=BestRaces;Username=*;Password=*;")
            .UseStringEnums();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Race>();

        modelBuilder.MapStringEnumAsPostgresEnum<Sport>();
    }
}
```

> `UseStringEnums()` is required in both modes — it teaches EF Core how to recognise `StringEnum<T>` properties as scalars rather than navigation properties. `MapStringEnumAsPostgresEnum<TEnum>` then overrides the column type to the Postgres enum.

`MapStringEnumAsPostgresEnum<TEnum>()` does two things:

1. Registers a Postgres enum type in the EF model, so a `CREATE TYPE` migration is produced. Labels are taken from the string enum's underlying values, in declaration order.
2. Walks all entity types and configures every property of type `TEnum` to use that Postgres enum as its column type, applying a value converter that maps a `Sport` member to its underlying string value.

The generated migration looks like this:

```csharp
migrationBuilder.AlterDatabase()
    .Annotation("Npgsql:Enum:sport", "ROAD_CYCLING,MTB,TRAIL_RUNNING");

migrationBuilder.CreateTable(
    name: "Races",
    columns: table => new
    {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        Name = table.Column<string>(type: "text", nullable: false),
        Sport = table.Column<Sport>(type: "sport", nullable: false)
    },
    constraints: table => table.PrimaryKey("PK_Races", x => x.Id));
```

#### Customising the Postgres enum name and schema

By default the Postgres enum name is the snake_cased CLR type name (`Sport` → `sport`). Override the name and schema if you need to:

```csharp
modelBuilder.MapStringEnumAsPostgresEnum<Sport>(name: "sport_kind", schema: "races");
```

#### Configuring individual properties

For fine-grained control over which properties map to a Postgres enum, call `HasPostgresStringEnum<TEnum>()` per property:

```csharp
modelBuilder.HasPostgresStringEnum<Sport>(); // creates the CREATE TYPE migration

modelBuilder.Entity<Race>()
    .Property(r => r.Sport)
    .HasPostgresStringEnum<Sport>();
```

`HasPostgresStringEnum<TEnum>()` on `ModelBuilder` only registers the type. The `PropertyBuilder` overload sets the column type and a value converter for that single property.

### Mixing both modes

Both modes can coexist in the same context:

```csharp
modelBuilder.MapStringEnumAsPostgresEnum<Sport>();    // Sport -> sport enum
// Country has no Postgres-enum mapping, so it stays as text
```

`Country` properties stay as `text` because `UseStringEnums()` is on the options builder.

### Querying

EF Core translates LINQ operations on string enums into SQL:

```csharp
var trailRuns = await context.Races
    .Where(r => r.Sport == Sport.TrailRunning)
    .ToArrayAsync();
```

When `Sport` is mapped to a Postgres enum, the parameter is sent and compared as that enum type.

```csharp
var cyclingSports = new[] { Sport.MountainBiking, Sport.RoadCycling };

var cyclingRaces = await context.Races
    .Where(r => cyclingSports.Contains(r.Sport))
    .ToArrayAsync();
```

## Acknowledgements

Built on top of [Npgsql.EntityFrameworkCore.PostgreSQL](https://github.com/npgsql/efcore.pg). For provider-agnostic EF Core support, see [StrEnum.EntityFrameworkCore](https://github.com/StrEnum/StrEnum.EntityFrameworkCore).

## License

Copyright &copy; 2026 [Dmytro Khmara](https://dmytrokhmara.com).

StrEnum is licensed under the [MIT license](LICENSE.txt).
