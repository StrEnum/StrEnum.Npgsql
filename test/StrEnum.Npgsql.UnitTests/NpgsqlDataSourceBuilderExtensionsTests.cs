using FluentAssertions;
using Npgsql;
using Xunit;

namespace StrEnum.Npgsql.UnitTests;

public class NpgsqlDataSourceBuilderExtensionsTests
{
    public class Sport : StringEnum<Sport>
    {
        public static readonly Sport TrailRunning = Define("TRAIL_RUNNING");
    }

    [Fact]
    public void MapStringEnum_ReturnsTheSameBuilderForChaining()
    {
        var builder = new NpgsqlDataSourceBuilder("Host=localhost");

        var result = builder.MapStringEnum<Sport>("sport", "races");

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void MapStringEnum_DoesNotThrowOnDefaultArguments()
    {
        var builder = new NpgsqlDataSourceBuilder("Host=localhost");

        var act = () => builder.MapStringEnum<Sport>();

        act.Should().NotThrow();
    }
}
