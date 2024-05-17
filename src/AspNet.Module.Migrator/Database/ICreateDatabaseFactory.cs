using AspNet.Module.Migrator.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNet.Module.Migrator.Database;

/// <summary>
///     Создание контекста БД
/// </summary>
public interface ICreateDatabaseFactory<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    ///     Создать новый экземпляр контекста БД
    /// </summary>
    TDbContext Create(string? connStr, IMigratorInternalConfig<TDbContext> internalConfig);
}