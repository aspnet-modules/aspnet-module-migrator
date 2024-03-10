using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace AspNet.Module.Migrator.Database;

/// <summary>
///     Создание контекста БД
/// </summary>
public interface ICreateDatabaseFactory<out TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    ///     Создать новый экземпляр контекста БД
    /// </summary>
    TDbContext Create(string? connStr, Action<NpgsqlDbContextOptionsBuilder>? configure = null);
}