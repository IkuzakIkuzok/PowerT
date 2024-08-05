
// (c) 2024 Kazuki Kohzuki

using System.Xml.Serialization;

namespace PowerT.Config;

/// <summary>
/// Wraps the <see cref="System.Drawing.Color"/> class to serialize it.
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
    /// <exception cref="Exception"><c>value</c> is not a valid HTML color name.</exception>
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

    public static implicit operator Color(SerializableColor color)
        => color.Color;

    public static implicit operator SerializableColor(Color color)
        => new() { Color = color };
} // public class SerializableColor
