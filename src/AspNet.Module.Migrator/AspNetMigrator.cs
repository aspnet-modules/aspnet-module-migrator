using AspNet.Module.Migrator.Console;
using AspNet.Module.Migrator.Web;
using Microsoft.EntityFrameworkCore;

namespace AspNet.Module.Migrator;

/// <summary>
///     Мигратор
/// </summary>
public static class AspNetMigrator
{
    /// <summary>
    ///     Для консоли
    /// </summary>
    public static IAspNetMigratorHost Console<TDbContext>(ConsoleMigratorConfig<TDbContext> config)
        where TDbContext : DbContext =>
        new ConsoleMigratorHost<TDbContext>(config);

    /// <summary>
    ///     Для веба
    /// </summary>
    public static IAspNetMigratorHost Web<TDbContext>(WebMigratorConfig<TDbContext> config)
        where TDbContext : DbContext =>
        new WebMigratorHost<TDbContext>(config);
}