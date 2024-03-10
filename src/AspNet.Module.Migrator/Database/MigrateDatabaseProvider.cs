using AspNet.Module.Migrator.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AspNet.Module.Migrator.Database;

/// <summary>
///     Миграция таблиц
/// </summary>
internal class MigrateDatabaseProvider<TDbContext>
    where TDbContext : DbContext
{
    private readonly ILogger<MigrateDatabaseProvider<TDbContext>> _logger;

    /// <summary>
    ///     Создание провайдера
    /// </summary>
    public MigrateDatabaseProvider(ILogger<MigrateDatabaseProvider<TDbContext>> logger)
    {
        _logger = logger;
    }

    public async Task Execute(TDbContext dbContext, CancellationToken ct)
    {
        _logger.LogInformation("Начинаю миграцию таблиц ----------->");
        await dbContext.Database.MigrateAsync(ct);
        dbContext.Database.ReloadEnumTypes();
        _logger.LogInformation("Миграция таблиц завершилась <-----------");
    }
}