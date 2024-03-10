using System.Data.Common;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace AspNet.Module.Migrator.Seeds;

/// <summary>
///     Хелпер популяции данных
/// </summary>
public interface ISeedDatabaseHelper
{
    /// <summary>
    ///     Рандом
    /// </summary>
    static Random Random { get; } = new();

    /// <summary>
    ///     Выполнить сырой SQL запрос
    /// </summary>
    Task<DbCommand> CreateSqlCommand(DbContext dbContext, string query, CancellationToken ct = default);

    /// <summary>
    ///     Выполнить сырой SQL запрос
    /// </summary>
    Task RawSqlQuery(DbContext dbContext, string query, CancellationToken ct = default);

    /// <summary>
    ///     Получить вложенный файл
    /// </summary>
    byte[]? ReadEmbeddedFile(string? name, Assembly? assembly = null);

    /// <summary>
    ///     Получить вложенный файл построчно
    /// </summary>
    IEnumerable<string> ReadEmbeddedFileLines(string? name, Assembly? assembly = null);

    /// <summary>
    ///     Получить вложенный json
    /// </summary>
    T? ReadEmbeddedJson<T>(string? name, Assembly? assembly = null, JsonSerializerOptions? options = null)
        where T : class;

    /// <summary>
    ///     Получить вложенный текст
    /// </summary>
    string ReadEmbeddedText(string? name, Assembly? assembly = null);
}