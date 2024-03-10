using Microsoft.Extensions.Configuration;

namespace AspNet.Module.Migrator.Database;

/// <summary>
///     Строка подключения к БД
/// </summary>
internal static class DbConnectionStr
{
    public static string FromConfiguration(IConfiguration configuration, string key = "Default")
    {
        var cs = configuration.GetConnectionString(key);
        return Normalize(cs) ?? throw new ArgumentNullException(nameof(configuration),
            $"Не указана строка подключения к БД в ConnectionStrings:{key}");
    }

    /// <summary>
    ///     Нормализация строки подключения
    /// </summary>
    /// <param name="connUrl">Сырая строка подключения</param>
    /// <returns>Нормализованная строка подключения</returns>
    public static string? Normalize(string? connUrl)
    {
        if (string.IsNullOrWhiteSpace(connUrl))
        {
            return null;
        }

        if (!connUrl.StartsWith("postgres://"))
        {
            return connUrl;
        }

        // Parse connection URL to connection string for Npgsql
        connUrl = connUrl.Replace("postgres://", string.Empty);

        var pgUserPass = connUrl.Split('@')[0];
        var pgHostPortDb = connUrl.Split('@')[1];
        var pgHostPort = pgHostPortDb.Split('/')[0];

        var pgDb = pgHostPortDb.Split('/')[1];
        var pgUser = pgUserPass.Split(':')[0];
        var pgPass = pgUserPass.Split(':')[1];
        var pgHost = pgHostPort.Split(':')[0];
        var pgPort = pgHostPort.Split(':')[1];

        // https://github.com/jincod/dotnetcore-buildpack/issues/33#issuecomment-397009370
        return
            $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};sslmode=Prefer;Trust Server Certificate=true";
    }
}