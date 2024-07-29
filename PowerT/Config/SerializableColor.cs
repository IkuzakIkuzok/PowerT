
// (c) 2024 Kazuki Kohzuki

using System.Xml.Serialization;

namespace PowerT.Config;

/// <summary>
/// Wraps the <see cref="Color"/> class to serialize it.
/// </summary>
[Serializable]
public class SerializableColor
{
    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    internal Color Color { get; set; }

    /// <summary>
    /// Gets or sets the string representation of the color.
    /// </summary>
    [XmlText]
    public string ColorString
    {
        get => $"#{this.Color.R:X2}{this.Color.G:X2}{this.Color.B:X2}";
        set => this.Color = ColorTranslator.FromHtml(value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializableColor"/> class.
    /// </summary>
    public SerializableColor() { }
} // public class SerializableColor
