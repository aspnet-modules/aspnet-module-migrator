using System.Reflection;
using AspNet.Module.Migrator.Models;
using Microsoft.EntityFrameworkCore;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace AspNet.Module.Migrator;

/// <summary>
///     Базовый контекст мигратора
/// </summary>
public class AspNetMigratorContext<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    ///     Вызываемый <see cref="Assembly" />
    /// </summary>
    public Assembly EntryAssembly { get; set; } = Assembly.GetEntryAssembly()!;

    /// <summary>
    ///     Регистрация модулей
    /// </summary>
    public Action<ModuleList>? Modules { get; set; }

    /// <summary>
    ///     Регистрация сидов
    /// </summary>
    public Action<SeedList<TDbContext>>? Seeds { get; set; }
}