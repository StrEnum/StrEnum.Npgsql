using System;
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace StrEnum.Npgsql.UnitTests;

public class ModelBuilderExtensionsTests
{
    public class Sport : StringEnum<Sport>
    {
        public static readonly Sport RoadCycling = Define("ROAD_CYCLING");
        public static readonly Sport MountainBiking = Define("MTB");
        public static readonly Sport TrailRunning = Define("TRAIL_RUNNING");
    }

    public class Race
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public Sport Sport { get; set; } = null!;
    }

    private class UncachedModelKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime) => Guid.NewGuid();
    }

    private class RaceContext : DbContext
    {
        private readonly Action<ModelBuilder>? _customize;

        public DbSet<Race> Races => Set<Race>();

        public RaceContext(Action<ModelBuilder>? customize = null)
        {
            _customize = customize;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql("Host=localhost;Database=tests")
                .UseStringEnums()
                .ReplaceService<IModelCacheKeyFactory, UncachedModelKeyFactory>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Race>();
            _customize?.Invoke(modelBuilder);
        }

        public IModel DesignTimeModel => this.GetService<IDesignTimeModel>().Model;
    }

    [Fact]
    public void HasPostgresStringEnum_ShouldRegisterEnumWithLabelsFromMembers()
    {
        using var context = new RaceContext(b => b.HasPostgresStringEnum<Sport>());

        var pgEnum = context.DesignTimeModel.GetPostgresEnums().Single();

        pgEnum.Name.Should().Be("sport");
        pgEnum.Schema.Should().BeNull();
        pgEnum.Labels.Should().Equal("ROAD_CYCLING", "MTB", "TRAIL_RUNNING");
    }

    [Fact]
    public void HasPostgresStringEnum_ShouldUseProvidedNameAndSchema()
    {
        using var context = new RaceContext(b => b.HasPostgresStringEnum<Sport>("sport_kind", "races"));

        var pgEnum = context.DesignTimeModel.GetPostgresEnums().Single();

        pgEnum.Name.Should().Be("sport_kind");
        pgEnum.Schema.Should().Be("races");
    }

    [Fact]
    public void MapStringEnumAsPostgresEnum_ShouldConfigureMatchingPropertiesWithConverterAndColumnType()
    {
        using var context = new RaceContext(b => b.MapStringEnumAsPostgresEnum<Sport>());

        var sportProperty = context.DesignTimeModel.FindEntityType(typeof(Race))!.FindProperty(nameof(Race.Sport))!;

        sportProperty.GetColumnType().Should().Be("sport");
        sportProperty.GetValueConverter().Should().NotBeNull();
        sportProperty.GetValueConverter()!.ConvertToProvider(Sport.MountainBiking).Should().Be("MTB");
    }

    [Fact]
    public void MapStringEnumAsPostgresEnum_ShouldQualifyColumnTypeWithSchema()
    {
        using var context = new RaceContext(b => b.MapStringEnumAsPostgresEnum<Sport>("sport", "races"));

        var sportProperty = context.DesignTimeModel.FindEntityType(typeof(Race))!.FindProperty(nameof(Race.Sport))!;

        sportProperty.GetColumnType().Should().Be("races.sport");
    }

    [Fact]
    public void MapStringEnumAsPostgresEnum_ShouldRegisterTheEnumInTheModel()
    {
        using var context = new RaceContext(b => b.MapStringEnumAsPostgresEnum<Sport>());

        context.DesignTimeModel.GetPostgresEnums().Should().ContainSingle(e => e.Name == "sport");
    }
}
