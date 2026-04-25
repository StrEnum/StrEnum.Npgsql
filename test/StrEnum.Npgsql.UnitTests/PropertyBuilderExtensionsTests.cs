using System;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Xunit;

namespace StrEnum.Npgsql.UnitTests;

public class PropertyBuilderExtensionsTests
{
    public class Country : StringEnum<Country>
    {
        public static readonly Country Ukraine = Define("UKR");
        public static readonly Country SouthAfrica = Define("ZAF");
    }

    public class Person
    {
        public Guid Id { get; set; }
        public Country Citizenship { get; set; } = null!;
    }

    [Fact]
    public void HasPostgresStringEnum_ShouldSetColumnTypeToSnakeCasedTypeName()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        modelBuilder.Entity<Person>().Property(p => p.Citizenship).HasPostgresStringEnum<Country>();

        var citizenship = modelBuilder.Model.FindEntityType(typeof(Person))!.FindProperty(nameof(Person.Citizenship))!;

        citizenship.GetColumnType().Should().Be("country");
    }

    [Fact]
    public void HasPostgresStringEnum_ShouldSetColumnTypeToProvidedName()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        modelBuilder.Entity<Person>().Property(p => p.Citizenship).HasPostgresStringEnum<Country>("country_iso");

        var citizenship = modelBuilder.Model.FindEntityType(typeof(Person))!.FindProperty(nameof(Person.Citizenship))!;

        citizenship.GetColumnType().Should().Be("country_iso");
    }

    [Fact]
    public void HasPostgresStringEnum_ShouldQualifyTheColumnTypeWithSchema()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        modelBuilder.Entity<Person>().Property(p => p.Citizenship).HasPostgresStringEnum<Country>("country", "geo");

        var citizenship = modelBuilder.Model.FindEntityType(typeof(Person))!.FindProperty(nameof(Person.Citizenship))!;

        citizenship.GetColumnType().Should().Be("geo.country");
    }

    [Fact]
    public void HasPostgresStringEnum_ShouldApplyAStringEnumValueConverter()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        modelBuilder.Entity<Person>().Property(p => p.Citizenship).HasPostgresStringEnum<Country>();

        var citizenship = modelBuilder.Model.FindEntityType(typeof(Person))!.FindProperty(nameof(Person.Citizenship))!;

        var converter = citizenship.GetValueConverter();

        converter.Should().NotBeNull();
        converter!.ConvertToProvider(Country.Ukraine).Should().Be("UKR");
        converter.ConvertFromProvider("ZAF").Should().Be(Country.SouthAfrica);
    }
}
