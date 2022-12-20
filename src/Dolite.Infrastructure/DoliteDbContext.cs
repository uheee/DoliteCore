using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dolite.Infrastructure;

public abstract class DoliteDbContext : DbContext
{
    private readonly DbConnection? _connection;
    private readonly string? _connectionString;

    public DoliteDbContext(DbConnection connection)
    {
        _connection = connection;
    }

    public DoliteDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    [ActivatorUtilitiesConstructor]
    public DoliteDbContext(DbContextOptions<DoliteDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        if (builder.IsConfigured) return;

        if (_connection is not null) builder.UseNpgsql(_connection);
        else if (_connectionString is not null) builder.UseNpgsql(_connectionString);

        builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
}