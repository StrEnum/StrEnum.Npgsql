using Testcontainers.PostgreSql;
using Xunit;

namespace StrEnum.Npgsql.IntegrationTests;

/// <summary>
/// xUnit fixture that boots a Postgres container per test class. Requires Docker on the host.
/// We isolate per class (rather than per assembly) so each class can call <c>EnsureCreatedAsync</c>
/// against a fresh database without colliding with other classes' models.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync() => await _container.StartAsync();

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
