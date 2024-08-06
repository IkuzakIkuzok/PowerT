
// (c) 2024 Kazuki Kohzuki

using System.Xml.Serialization;

namespace PowerT.Config;

/// <summary>
/// Represents the appearance configuration.
/// </summary>
[Serializable]
public sealed class AppearanceConfig
{
    /// <summary>
    /// Gets or sets the color gradient configuration.
    /// </summary>
    [XmlElement("color-gradient")]
    public ColorGradientConfig ColorGradientConfig { get; set; } = new();

    /// <summary>
    /// Gets or sets the line width of the fit lines.
    /// </summary>
    [XmlElement("fit-width")]
    public int FitWidth { get; set; } = 3;

    /// <summary>
    /// Gets or sets the font of the axis labels.
    /// </summary>
    [XmlElement("axis-label")]
    public FontConfig AxisLabelFont { get; set; } = new();

    /// <summary>
    /// Gets or sets the font of the axis title.
    /// </summary>
    [XmlElement("axis-title")]
    public FontConfig AxisTitleFont { get; set; } = new();

    /// <summary>
    /// Gets or sets the color of the guide line.
    /// </summary>
    public SerializableColor GuideLineColor { get; set; } = Color.Black;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppearanceConfig"/> class.
    /// </summary>
    public AppearanceConfig() { }
} // public sealed class AppearanceConfig
