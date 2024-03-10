using System.Collections;
using AspNet.Module.Common;

namespace AspNet.Module.Migrator.Models;

/// <summary>
///     Лист регистрации модулей
/// </summary>
public class ModuleList : IEnumerable<IAspNetConfigureServicesModule>
{
    private readonly IDictionary<Type, IAspNetConfigureServicesModule> _modulesMap =
        new Dictionary<Type, IAspNetConfigureServicesModule>();

    /// <summary>
    ///     Регистрация модуля
    /// </summary>
    public ModuleList RegisterModule(IAspNetConfigureServicesModule module)
    {
        _modulesMap[module.GetType()] = module;
        return this;
    }

    /// <summary>
    ///     Регистрация модуля
    /// </summary>
    public ModuleList RegisterModule<TModule>() where TModule : IAspNetConfigureServicesModule, new()
    {
        _modulesMap[typeof(TModule)] = new TModule();
        return this;
    }

    /// <inheritdoc />
    public IEnumerator<IAspNetConfigureServicesModule> GetEnumerator() => _modulesMap.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}