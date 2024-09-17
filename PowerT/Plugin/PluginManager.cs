
// (c) 2024 Kazuki Kohzuki

using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace PowerT.Plugin;

/// <summary>
/// Manages the plugins.
/// </summary>
internal static class PluginManager
{
    private static readonly string pluginsDirectory;

    private static readonly List<IPlugin> plugins = [];

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
    internal static IReadOnlyList<IPlugin> Plugins => plugins;

    /// <summary>
    /// Gets the smoothers.
    /// </summary>
    internal static IReadOnlyList<ISmoother> Smoothers => plugins.OfType<ISmoother>().ToList();

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
            if (type.IsInterface || type.IsAbstract) continue;

            /*
             * Check if the type implements the IPlugin interface.
             * This can be omitted because the type without the IPlugin interface will be rejected
             * by `Activator.CreateInstance(type) is not IPlugin plugin`.
             * However, `Activator.CreateInstance` is a heavy operation, so it is better to check it here.
             */
            if (!typeof(IPlugin).IsAssignableFrom(type)) continue;
            try
            {
                if (Activator.CreateInstance(type) is not IPlugin plugin) continue;
                AddPlugin(plugin);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    } // internal static void Load (Assembly)

    /// <summary>
    /// Adds the plugin.
    /// </summary>
    /// <param name="plugin"></param>
    internal static void AddPlugin(IPlugin plugin)
    {
        plugin.Initialize();
        plugins.Add(plugin);
    } // internal static void AddPlugin (IPlugin)

    /// <summary>
    /// Unloads all plugins.
    /// </summary>
    internal static void UnloadAll()
    {
        foreach (var plugin in plugins)
            plugin.Dispose();
        plugins.Clear();
    } // internal static void UnloadAll ()
} // internal static class PluginManager
