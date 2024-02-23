
// (c) 2024 Kazuki Kohzuki

using System.Xml.Serialization;

namespace PowerT.Config;

[Serializable]
public sealed class FontConfig
{
    /// <summary>
    /// Gets or sets the name of the font.
    /// </summary>
    [XmlElement("name")]
    public string Name { get; set; } = "Arial";

    /// <summary>
    /// Gets or sets the size of the font.
    /// </summary>
    [XmlElement("size")]
    public float Size { get; set; } = 12.0f;

    /// <summary>
    /// Gets or sets the style of the font.
    /// </summary>
    [XmlElement("style")]
    public FontStyle Style { get; set; } = FontStyle.Regular;

    /// <summary>
    /// Gets or sets the unit of the font.
    /// </summary>
    public GraphicsUnit Unit { get; set; } = GraphicsUnit.Point;

    [XmlIgnore]
    public Font Font
    {
        get => new(this.Name, this.Size, this.Style, this.Unit);
        set
        {
            this.Name = value.Name;
            this.Size = value.Size;
            this.Style = value.Style;
            this.Unit = value.Unit;
        }
    }

    public FontConfig() { }

    public FontConfig(Font font)
    {
        this.Font = font;
    } // ctor (Font)
} // public sealed class FontConfig
