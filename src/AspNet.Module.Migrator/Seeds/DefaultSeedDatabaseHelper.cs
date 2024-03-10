using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace AspNet.Module.Migrator.Seeds;

/// <inheritdoc />
internal class DefaultSeedDatabaseHelper : ISeedDatabaseHelper
{
    private readonly Assembly _entryAssembly;

    public DefaultSeedDatabaseHelper(Assembly entryAssembly)
    {
        _entryAssembly = entryAssembly;
    }

    public async Task<DbCommand> CreateSqlCommand(DbContext dbContext, string query, CancellationToken ct = default)
    {
        var command = dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        command.CommandType = CommandType.Text;
        await dbContext.Database.OpenConnectionAsync(ct);
        return command;
    }

    /// <inheritdoc />
    public async Task RawSqlQuery(DbContext dbContext, string query,
        CancellationToken ct = default)
    {
        await using var command = await CreateSqlCommand(dbContext, query, ct);
        command.CommandText = query;
        command.CommandType = CommandType.Text;
        await using var result = await command.ExecuteReaderAsync(ct);
    }

    /// <inheritdoc />
    public byte[]? ReadEmbeddedFile(string? name, Assembly? assembly = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        assembly ??= _entryAssembly;
        var resourcePath = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith(name));
        if (resourcePath == null)
        {
            return null;
        }

        using var stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
        {
            return null;
        }

        var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    /// <inheritdoc />
    public IEnumerable<string> ReadEmbeddedFileLines(string? name, Assembly? assembly = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            yield break;
        }

        assembly ??= _entryAssembly;
        var resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));
        using var stream = assembly.GetManifestResourceStream(resourcePath);
        using var reader = new StreamReader(stream!, Encoding.UTF8);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            yield return line;
        }
    }

    /// <inheritdoc />
    public T? ReadEmbeddedJson<T>(string? name, Assembly? assembly, JsonSerializerOptions? options)
        where T : class
    {
        var text = ReadEmbeddedText(name, assembly);
        return JsonSerializer.Deserialize<T>(text, options);
    }

    /// <inheritdoc />
    public string ReadEmbeddedText(string? name, Assembly? assembly = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }

        assembly ??= _entryAssembly;
        var resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));
        using var stream = assembly.GetManifestResourceStream(resourcePath);
        using var reader = new StreamReader(stream!, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}