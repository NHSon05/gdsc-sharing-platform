using Npgsql;

namespace GdscSharingPlatform.IntegrationTests.Support;

/// <summary>Owns a generated test database; never migrates or drops the configured database.</summary>
internal sealed class PostgresTestDatabase
{
    private readonly string _name = $"roadmap_api_{Guid.NewGuid():N}";
    private readonly string _adminConnectionString;
    private bool _created;
    public string ConnectionString { get; }

    private PostgresTestDatabase(string configured)
    {
        var builder = new NpgsqlConnectionStringBuilder(configured) { Database = "postgres", Pooling = false };
        _adminConnectionString = builder.ConnectionString;
        builder.Database = _name;
        ConnectionString = builder.ConnectionString;
    }

    public static PostgresTestDatabase? FromEnvironment()
    {
        var configured = Environment.GetEnvironmentVariable("ROADMAP_TEST_POSTGRES");
        return string.IsNullOrWhiteSpace(configured) ? null : new(configured);
    }

    public async Task CreateAsync()
    {
        await using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{_name}\"", connection);
        await command.ExecuteNonQueryAsync();
        _created = true;
    }

    public async Task DropAsync()
    {
        if (!_created) return;
        await using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE \"{_name}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
        _created = false;
    }
}
