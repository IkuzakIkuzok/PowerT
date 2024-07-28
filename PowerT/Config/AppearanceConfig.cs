
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
    /// Gets or sets the line width of the fitted lines.
    /// </summary>
    [XmlElement("fitted-width")]
    public int FittedWidth { get; set; } = 3;

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

    public SerializableColor GuideLineColor { get; set; } = new() { Color = Color.Black };

    public AppearanceConfig() { }
} // public sealed class AppearanceConfig
