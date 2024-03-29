using AutoMapper;
using Dolite.Application.Error;
using Dolite.Application.Localization;
using Dolite.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Serilog;

namespace Dolite.Application.Service;

public class BaseService
{
    public IMapper Mapper { get; init; } = null!;
}

public class BaseService<TService> : BaseService, ICulturalResource<TService>
    where TService : BaseService<TService>
{
    public IStringLocalizer<TService> Localizer { get; init; } = null!;

    public BusinessException Error(int errCode, params object[] args)
    {
        var errTemplate = Localizer[errCode.ToString()];
        var errMsg = string.Format(errTemplate, args);
        if (string.IsNullOrEmpty(errMsg)) errMsg = "unknown";
        return new BusinessException(errCode, errMsg);
    }
}

public class BaseService<TService, TDbContext> : BaseService<TService>
    where TService : BaseService<TService, TDbContext>
    where TDbContext : DbContext
{
    public TDbContext DbContext { get; init; } = null!;
    public Lazy<DbContextProvider<TDbContext>> DbContextProviderLazier { get; init; } = null!;
    public DbContextProvider<TDbContext> DbContextProvider => DbContextProviderLazier.Value;

    #region Transaction

    protected Task UseTransaction(Func<TDbContext, Task> action)
    {
        return UseTransaction(async provider => await action(await provider.CreateAsync()));
    }

    protected Task<TResult> UseTransaction<TResult>(Func<TDbContext, Task<TResult>> action)
    {
        return UseTransaction(async provider => await action(await provider.CreateAsync()));
    }

    protected async Task UseTransaction(Func<DbContextProvider<TDbContext>, Task> action)
    {
        try
        {
            await action(DbContextProvider);
            if (DbContextProvider.Transaction is null)
                throw new Exception("DbContext count is less than 1");
            await DbContextProvider.Transaction.CommitAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to execute transaction");
            if (DbContextProvider.Transaction is not null)
                await DbContextProvider.Transaction.RollbackAsync();
            throw;
        }
    }

    protected async Task<TResult> UseTransaction<TResult>(Func<DbContextProvider<TDbContext>, Task<TResult>> action)
    {
        try
        {
            var result = await action(DbContextProvider);
            if (DbContextProvider.Transaction is null)
                throw new Exception("DbContext count is less than 1");
            await DbContextProvider.Transaction.CommitAsync();
            return result;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to execute transaction");
            if (DbContextProvider.Transaction is not null)
                await DbContextProvider.Transaction.RollbackAsync();
            throw;
        }
    }

    #endregion
}