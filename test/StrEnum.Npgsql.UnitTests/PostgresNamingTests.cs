using FluentAssertions;
using Xunit;

namespace StrEnum.Npgsql.UnitTests;

public class PostgresNamingTests
{
    [Theory]
    [InlineData("Sport", "sport")]
    [InlineData("RaceSport", "race_sport")]
    [InlineData("HTTPRequest", "http_request")]
    [InlineData("DBMS", "dbms")]
    [InlineData("camelCase", "camel_case")]
    [InlineData("", "")]
    public void ToSnakeCase_ShouldConvertIdentifiersToSnakeCase(string input, string expected)
    {
        PostgresNaming.ToSnakeCase(input).Should().Be(expected);
    }
}
