using FluentAssertions;
using Xunit;

namespace StrEnum.Npgsql.UnitTests;

public class StringEnumLabelsTests
{
    public class Sport : StringEnum<Sport>
    {
        public static readonly Sport RoadCycling = Define("ROAD_CYCLING");
        public static readonly Sport MountainBiking = Define("MTB");
        public static readonly Sport TrailRunning = Define("TRAIL_RUNNING");
    }

    [Fact]
    public void For_ShouldReturnUnderlyingValuesInDeclarationOrder()
    {
        var labels = StringEnumLabels.For<Sport>();

        labels.Should().Equal("ROAD_CYCLING", "MTB", "TRAIL_RUNNING");
    }

    [Fact]
    public void For_ShouldReturnTheSameInstanceOnSubsequentCalls()
    {
        var first = StringEnumLabels.For<Sport>();
        var second = StringEnumLabels.For<Sport>();

        first.Should().BeSameAs(second);
    }
}
