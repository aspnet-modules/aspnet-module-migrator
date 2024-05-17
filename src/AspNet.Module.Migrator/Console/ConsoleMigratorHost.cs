using CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AspNet.Module.Migrator.Console;

internal class ConsoleMigratorHost<TDbContext> : BaseMigratorHost<TDbContext>, IAspNetMigratorHost
    where TDbContext : DbContext
{
    private readonly ConsoleMigratorConfig<TDbContext> _config;

    public ConsoleMigratorHost(ConsoleMigratorConfig<TDbContext> config)
    {
        _config = config;
    }

    /// <inheritdoc />
    public async Task Execute(CancellationToken ct)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var result = await Parser.Default
            .ParseArguments<ConsoleMigratorArgs>(_config.Args)
            .WithParsedAsync(a => RunWithOptions(_config, a));
        await result.WithNotParsedAsync(ParseError);
    }

    private IHostBuilder CreateHostBuilder(ConsoleMigratorConfig<TDbContext> config)
    {
        var hostBuilder = Host.CreateDefaultBuilder(config.Args)
            .ConfigureHostConfiguration(cb =>
            {
                cb.AddCommandLine(config.Args);
                cb.AddJsonFile("appsettings.json", true);
                cb.AddEnvironmentVariables();
                config.Configuration?.Invoke(cb);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton(config);
                ConfigureServices(services, hostContext.Configuration, hostContext.HostingEnvironment, _config.Context);
            })
            .UseSerilog((hbc, lc) => ConfigureLogger(lc, hbc.Configuration, hbc.HostingEnvironment));

        return hostBuilder;
    }

    private async Task RunWithOptions(ConsoleMigratorConfig<TDbContext> config, ConsoleMigratorArgs args)
    {
        using var host = CreateHostBuilder(config).Build();
        await using var hostScope = host.Services.CreateAsyncScope();
        var cts = new CancellationTokenSource(TimeSpan.FromHours(1));
        using var scope = hostScope.ServiceProvider.CreateScope();
        await MigrateAndSeed(scope, args.ConnectionString, config, args.Seed, cts.Token);
    }

    private static Task ParseError(IEnumerable<Error> errs)
    {
        System.Console.WriteLine($"Parse errors: {string.Join(',', errs.Select(x => x.Tag))}", errs);
        return Task.CompletedTask;
    }
}