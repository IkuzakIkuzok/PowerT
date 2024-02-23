
// (c) 2024 Kazuki Kohzuki

using System.Xml.Serialization;

namespace PowerT.Config;

[Serializable]
public class SerializableColor
{
    internal Color Color { get; set; }

    [XmlText]
    public string ColorString
    {
        get => $"#{this.Color.R:X2}{this.Color.G:X2}{this.Color.B:X2}";
        set => this.Color = ColorTranslator.FromHtml(value);
    }

    public SerializableColor() { }
} // public class SerializableColor
