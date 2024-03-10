using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AspNet.Module.Migrator.Web;

internal class WebMigratorHost<TDbContext> : BaseMigratorHost<TDbContext>, IAspNetMigratorHost
    where TDbContext : DbContext
{
    private readonly WebMigratorConfig<TDbContext> _config;

    public WebMigratorHost(WebMigratorConfig<TDbContext> config)
    {
        _config = config;
    }

    /// <inheritdoc />
    public async Task Execute(CancellationToken ct)
    {
        var sc = new ServiceCollection();
        var lc = ConfigureLogger(new LoggerConfiguration(), _config.Configuration, _config.Env);
        ConfigureServices(sc, _config.Configuration, _config.Env, _config.Context);
        var logger = lc.CreateLogger();
        sc.AddLogging(logging => { logging.AddSerilog(logger); });

        var sp = sc.BuildServiceProvider();
        using var scope = sp.CreateScope();

        await MigrateAndSeed(scope, _config.ConnStr, _config.NpgsqlConfigure, _config.Seed, ct);
    }
}