
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Plugin;

/// <summary>
/// Represents a plugin.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the plugin.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes the plugin.
    /// </summary>
    public void Initialize();

    /// <summary>
    /// Disposes the plugin.
    /// </summary>
    public void Dispose();
} // public interface IPlugin
