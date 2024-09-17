
// (c) 2024 Kazuki Kohzuki

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace PowerT.Plugin;

/// <summary>
/// Manages the plugins.
/// </summary>
internal static class PluginManager
{
    private static readonly string pluginsDirectory;

    private static readonly Dictionary<Guid, PluginItem> plugins = [];

    static PluginManager()
    {
        pluginsDirectory = Path.Combine(Program.AppLocation, "plugins");

        try
        {
            // load built-in plugins
            Load(Assembly.GetExecutingAssembly());

            if (!Directory.Exists(pluginsDirectory)) return;
            foreach (var file in Directory.EnumerateFiles(pluginsDirectory, "*.dll"))
            {
                try
                {
                    Load(file);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }
        catch
        {
            // ignore
        }
    } // cctor ()

    /// <summary>
    /// Gets the plugins.
    /// </summary>
    internal static IReadOnlyList<IPlugin> Plugins => plugins.Where(p => p.Value.Enabled).Select(p => p.Value.Instance).ToList();

    /// <summary>
    /// Gets the smoothers.
    /// </summary>
    internal static IReadOnlyList<ISmoother> Smoothers => Plugins.OfType<ISmoother>().ToList();

    /// <summary>
    /// Loads the plugin from the specified assembly path.
    /// </summary>
    /// <param name="assemblyPath">The fully qualified path of the file to load.</param>
    /// <exception cref="FileLoadException">A file that was found could not be loaded.</exception>
    /// <exception cref="FileNotFoundException">The <paramref name="assemblyPath"/> argument is an empty string ("") or does not exist.</exception>
    /// <exception cref="BadImageFormatException">The assemblyPath argument is not a valid assembly.</exception>
    /// <exception cref="ReflectionTypeLoadException">The assembly contains one or more types that cannot be loaded.</exception>
    internal static void Load(string assemblyPath)
    {
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
        Load(assembly);
    } // internal static void Load (string)

    /// <summary>
    /// Loads the plugin from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <exception cref="ReflectionTypeLoadException">The assembly contains one or more types that cannot be loaded.</exception>
    internal static void Load(Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            var guid = type.GUID;
            if (plugins.ContainsKey(guid)) continue;

            if (type.IsInterface || type.IsAbstract) continue;

            /*
             * Check if the type implements the IPlugin interface.
             * This can be omitted because the type without the IPlugin interface will be rejected `TryGetInstance`.
             * However, `Activator.CreateInstance` is a heavy operation, so it is better to check it here.
             */
            if (!typeof(IPlugin).IsAssignableFrom(type)) continue;
            if (!TryGetInstance(type, out var plugin)) continue;
            AddPlugin(plugin, guid);
        }
    } // internal static void Load (Assembly)
    
    /// <summary>
    /// Tries to create an instance of the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="plugin">The plugin instance.</param>
    /// <returns><see langword="true"/> if the instance is created successfully; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetInstance(Type type, [NotNullWhen(true)] out IPlugin? plugin)
    {
        try
        {
            plugin = Activator.CreateInstance(type) as IPlugin;
            return plugin != null;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            plugin = null;
            return false;
        }
    } // private static bool TryGetInstance (Type, out IPlugin)

    /// <summary>
    /// Adds the plugin.
    /// </summary>
    /// <param name="plugin"></param>
    private static void AddPlugin(IPlugin plugin, Guid guid)
    {
        plugin.Initialize();
        plugins.Add(guid, new(plugin));
    } // private static void AddPlugin (IPlugin)

    /// <summary>
    /// Unloads all plugins.
    /// </summary>
    internal static void UnloadAll()
    {
        foreach (var plugin in plugins.Values)
            plugin.Instance.Dispose();
        plugins.Clear();
    } // internal static void UnloadAll ()
} // internal static class PluginManager
