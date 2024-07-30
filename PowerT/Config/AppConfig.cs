
// (c) 2024 Kazuki Kohzuki

using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace PowerT.Config;

/// <summary>
/// Represents the application configuration.
/// </summary>
[Serializable]
[XmlRoot("appSettings")]
public sealed class AppConfig
{
    private const string FILENAME = "PowerT.config";

    private static readonly string FullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, FILENAME);

    /// <summary>
    /// Gets or sets the appearance configuration.
    /// </summary>
    [XmlElement("appearance")]
    public AppearanceConfig AppearanceConfig { get; set; } = new();

    /// <summary>
    /// Gets or sets the decay loading configuration.
    /// </summary>
    [XmlElement("decay-loading")]
    public DecayLoadingConfig DecayLoadingConfig { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AppConfig"/> class.
    /// </summary>
    public AppConfig() { }

    /// <summary>
    /// Loads the application configuration.
    /// </summary>
    /// <returns>Load the configuration from file.
    /// If the file does not exists, returns default configuration.</returns>
    internal static AppConfig Load()
    {
        if (!File.Exists(FullPath)) return new AppConfig();
        using var reader = new StreamReader(FullPath, Encoding.UTF8);
        return (AppConfig)new XmlSerializer(typeof(AppConfig)).Deserialize(reader)!;
    } // internal static AppConfig Load ()

    /// <summary>
    /// Saves the application configuration.
    /// </summary>
    internal void Save()
    {
        using var writer = new StreamWriter(FullPath, false, Encoding.UTF8);
        new XmlSerializer(typeof(AppConfig)).Serialize(writer, this);
    } // internal void Save ()
} // public sealed class AppConfig
