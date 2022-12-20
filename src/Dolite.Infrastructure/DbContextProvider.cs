using System.Data.Common;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Dolite.Infrastructure;

public class DbContextProvider<TDbContext> : IDisposable where TDbContext : DbContext
{
    private readonly Func<DbConnection, TDbContext> _dbContextBuilder;
    private readonly List<TDbContext> _dbContexts = new();

    public DbConnection DbConnection { get; init; } = null!;
    public DbTransaction Transaction { get; init; } = null!;

    public DbContextProvider(Expression<Func<DbConnection, TDbContext>> dbContextBuilder)
    {
        _dbContextBuilder = dbContextBuilder.Compile();
    }

    public void Dispose()
    {
        _dbContexts.ForEach(dbContext => dbContext.Dispose());
    }

    public TDbContext Create()
    {
        var dbContext = _dbContextBuilder(DbConnection);
        dbContext.Database.UseTransaction(Transaction);
        _dbContexts.Add(dbContext);
        return dbContext;
    }

    public async Task<TDbContext> CreateAsync()
    {
        var dbContext = _dbContextBuilder(DbConnection);
        await dbContext.Database.UseTransactionAsync(Transaction);
        _dbContexts.Add(dbContext);
        return dbContext;
    }
}