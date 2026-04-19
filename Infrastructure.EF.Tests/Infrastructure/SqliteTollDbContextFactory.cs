using EFCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EF.Tests.Infrastructure;

/// <summary>
/// IDbContextFactory baserad på SQLite InMemory. Håller en delad SqliteConnection öppen
/// så att schemat och datan lever så länge factoryn lever – varje test-instans får en
/// egen, isolerad databas. Till skillnad från EF Cores InMemory-provider respekterar
/// SQLite relationella constraints (t.ex. unika index).
/// </summary>
internal sealed class SqliteTollDbContextFactory : IDbContextFactory<TollDbContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<TollDbContext> _options;

    public SqliteTollDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<TollDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var ctx = CreateDbContext();
        ctx.Database.EnsureCreated();
    }

    public TollDbContext CreateDbContext() => new(_options);

    public Task<TollDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());

    public void Dispose() => _connection.Dispose();
}
