
// (c) 2024 Kazuki Kohzuki

using System.Xml.Serialization;

namespace PowerT.Config;

[Serializable]
public sealed class ColorGradientConfig
{
    [XmlElement("start")]
    public SerializableColor StartColor { get; set; } = new() { Color = Color.Red };

    [XmlElement("end")]
    public SerializableColor EndColor { get; set; } = new() { Color = Color.Blue };

    public ColorGradientConfig() { }
} // public sealed class ColorGradientConfig
