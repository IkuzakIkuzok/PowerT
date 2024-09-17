
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Plugin;

/// <summary>
/// Contains a plugin instance and its status.
/// </summary>
internal class PluginItem
{
    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    internal IPlugin Instance { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the plugin is enabled.
    /// </summary>
    /// <remarks>
    /// `IPlugin` interface must not have a property to enable/disable the plugin
    /// because it should be managed by the main application and not by the plugin itself.
    /// </remarks>
    internal bool Enabled { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginItem"/> class.
    /// </summary>
    /// <param name="instance">The plugin instance.</param>
    internal PluginItem(IPlugin instance)
    {
        this.Instance = instance;
        this.Enabled = true;
    } // ctor (IPlugin)
} // internal class PluginItem
