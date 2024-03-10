using Microsoft.EntityFrameworkCore;

namespace AspNet.Module.Migrator.Seeds;

/// <summary>
///     Интерфейс популяции данных в БД
/// </summary>
public interface ISeedDatabaseExecutor<in TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    ///     Включен?
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    ///     Название
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Порядок
    /// </summary>
    int Order { get; }

    /// <summary>
    ///     Выполнить популяцию
    /// </summary>
    Task Execute(TDbContext dbContext, CancellationToken ct);
}