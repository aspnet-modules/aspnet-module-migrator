using CommandLine;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace AspNet.Module.Migrator.Console;

/// <summary>
///     Аргументы скрипта
/// </summary>
internal class ConsoleMigratorArgs
{
    /// <summary>
    ///     Строка подключения к БД
    /// </summary>
    [Option('c', "connection", Required = false, HelpText = "Строка подключения к БД или использовать переменные")]
    public string? ConnectionString { get; set; }

    /// <summary>
    ///     Пополнить данные
    /// </summary>
    [Option('s', "seed", Required = false, Default = true, HelpText = "Популяция БД после миграции")]
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public bool Seed { get; set; } = true;
}