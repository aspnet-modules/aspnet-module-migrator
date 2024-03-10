using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace AspNet.Module.Migrator.Seeds;

/// <summary>
///     Провайдер популяции данных
/// </summary>
internal class SeedDatabaseProvider<TDbContext>
    where TDbContext : DbContext
{
    private const string SeedsTable = "__ef_seeds_history";
    private readonly ILogger<SeedDatabaseProvider<TDbContext>> _logger;
    private readonly ISeedDatabaseExecutor<TDbContext>[] _seedDatabaseExecutors;
    private readonly ISeedDatabaseHelper _seedDatabaseHelper;

    public SeedDatabaseProvider(IEnumerable<ISeedDatabaseExecutor<TDbContext>> seedDatabaseExecutors,
        ILogger<SeedDatabaseProvider<TDbContext>> logger, ISeedDatabaseHelper seedDatabaseHelper)
    {
        _seedDatabaseExecutors = seedDatabaseExecutors
            .Where(x => x.Enabled)
            .OrderBy(x => x.Order)
            .ToArray();
        _logger = logger;
        _seedDatabaseHelper = seedDatabaseHelper;
    }

    public async Task Execute(TDbContext dbContext, CancellationToken ct)
    {
        var productVersion = typeof(DatabaseFacade).Assembly.GetName().Version?.ToString() ?? string.Empty;
        await CreateSeedsTablesIfNeeded(dbContext, ct);
        _logger.LogInformation("Началась популяция данных ----------->");
        if (_seedDatabaseExecutors.Length == 0)
        {
            _logger.LogDebug("Нет данных для популяции <----->");
        }

        foreach (var seedExecutor in _seedDatabaseExecutors)
        {
            var seedExecutorName = seedExecutor.Name;
            _logger.LogDebug($"Популятор {seedExecutorName}: Запускаю популяцию данных ----->");

            if (await SeedNameExists(dbContext, seedExecutorName, ct))
            {
                _logger.LogDebug($"Популятор {seedExecutorName}: Уже выполнен...");
                continue;
            }

            try
            {
                await seedExecutor.Execute(dbContext, ct);
                await dbContext.SaveChangesAsync(ct);

                await _seedDatabaseHelper.RawSqlQuery(dbContext,
                    $"insert into \"{SeedsTable}\" (\"seed_id\",\"product_version\") VALUES('{seedExecutor.Name}','{productVersion}') ON CONFLICT (\"seed_id\") DO NOTHING;",
                    ct);

                _logger.LogDebug($"Популятор {seedExecutorName}: Популяция данных завершена <-----");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Не удалось выполнить популяцию с названием {seedExecutorName}");
                throw;
            }
        }

        _logger.LogInformation("Завершилась Популяция данных <-----------");
    }

    private async Task CreateSeedsTablesIfNeeded(DbContext dbContext, CancellationToken ct)
    {
        var sql = _seedDatabaseHelper.ReadEmbeddedText("CreateSeedsTables.sql", GetType().Assembly);
        await _seedDatabaseHelper.RawSqlQuery(dbContext, sql, ct);
    }

    private async Task<bool> SeedNameExists(DbContext dbContext, string seedExecutorName, CancellationToken ct)
    {
        var command = await _seedDatabaseHelper.CreateSqlCommand(dbContext,
            $"select count(1) from \"{SeedsTable}\" where \"seed_id\"='{seedExecutorName}';",
            ct);
        await using var result = await command.ExecuteReaderAsync(ct);
        var exists = false;
        while (await result.ReadAsync(ct))
        {
            exists = result.GetInt64(0) > 0;
        }

        return exists;
    }
}