using System;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace StrEnum.Npgsql.UnitTests;

public class DbContextOptionsBuilderExtensionsTests
{
    public class Sport : StringEnum<Sport>
    {
        public static readonly Sport TrailRunning = Define("TRAIL_RUNNING");
        public static readonly Sport RoadCycling = Define("ROAD_CYCLING");
    }

    public class Race
    {
        public Guid Id { get; set; }
        public Sport Sport { get; set; } = null!;
    }

    public class RaceContext : DbContext
    {
        public DbSet<Race> Races => Set<Race>();

        public RaceContext(DbContextOptions<RaceContext> options) : base(options)
        {
        }
    }

    [Fact]
    public void UseStringEnums_ShouldReplaceTheValueConverterSelectorSoStringEnumsAreHandled()
    {
        var options = new DbContextOptionsBuilder<RaceContext>()
            .UseInMemoryDatabase("strenum-npgsql-tests")
            .UseStringEnums()
            .Options;

        using var context = new RaceContext(options);

        var selector = context.GetService<IValueConverterSelector>();

        selector.Select(typeof(Sport), typeof(string)).Should().NotBeEmpty();
    }
}
