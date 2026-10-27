using System.Data.Common;
using Npgsql;

namespace Blueprint.Framework.Database;

/// <summary>
/// Optional direct database access for setup, clean-up and verification when the app offers no API for it.
/// Thread-safe: share one instance per run. Uses PostgreSQL through Npgsql; to target another engine, change
/// <see cref="Create"/> to build that provider's <see cref="DbDataSource"/> (SqlClient, MySqlConnector, ...).
/// Always pass values as parameters, never by concatenating SQL.
/// </summary>
/// <example><c>await db.ExecuteAsync("DELETE FROM users WHERE email = @email", ("email", user.Email));</c></example>
public sealed class DatabaseClient(DbDataSource dataSource) : IAsyncDisposable
{
    public static DatabaseClient Create(string connectionString) => new(NpgsqlDataSource.Create(connectionString));

    /// <summary>Runs a command and returns the number of affected rows.</summary>
    public async Task<int> ExecuteAsync(string sql, params (string Name, object? Value)[] parameters)
    {
        await using var command = CreateCommand(sql, parameters);
        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>Runs a query and returns the first column of the first row, or default when there is none.</summary>
    public async Task<T?> ScalarAsync<T>(string sql, params (string Name, object? Value)[] parameters)
    {
        await using var command = CreateCommand(sql, parameters);
        var result = await command.ExecuteScalarAsync();
        return result is null or DBNull ? default : (T)result;
    }

    public ValueTask DisposeAsync() => dataSource.DisposeAsync();

    private DbCommand CreateCommand(string sql, (string Name, object? Value)[] parameters)
    {
        var command = dataSource.CreateCommand(sql);

        foreach (var (name, value) in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        return command;
    }
}
