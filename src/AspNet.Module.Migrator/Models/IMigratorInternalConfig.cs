using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace AspNet.Module.Migrator.Models;

/// <summary>
///     Контекст миграции
/// </summary>
public interface IMigratorInternalConfig<TDbContext> where TDbContext : DbContext
{
    /// <summary>
    ///     Конфигурация <see cref="DbContextOptionsBuilder" />
    /// </summary>
    public Action<DbContextOptionsBuilder<TDbContext>>? DbContext { get; }

    /// <summary>
    ///     Схема для истории миграций
    /// </summary>
    public string MigratorSchema { get; }

    /// <summary>
    ///     Конфигурация <see cref="NpgsqlDbContextOptionsBuilder" />
    /// </summary>
    public Action<NpgsqlDbContextOptionsBuilder>? NpgsqlContext { get; }
}