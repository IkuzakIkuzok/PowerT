
// (c) 2024 Kazuki Kohzuki

using System.Xml.Serialization;

namespace PowerT.Config;

/// <summary>
/// Represents the decay loading configuration.
/// </summary>
[Serializable]
public sealed class DecayLoadingConfig
{
    /// <summary>
    /// Gets or sets the filename format of the A-B signal.
    /// </summary>
    [XmlElement("format-a-b")]
    public string AMinusBSignalFormat { get; set; } = "<BASENAME>-a-b-tdm.csv";

    /// <summary>
    /// Gets or sets the filename format of the B signal.
    /// </summary>
    [XmlElement("format-b")]
    public string BSignalFormat { get; set; } = "<BASENAME>-b.csv";

    /// <summary>
    /// Initializes a new instance of the <see cref="DecayLoadingConfig"/> class.
    /// </summary>
    public DecayLoadingConfig() { }
} // public sealed class DecayLoadingConfig
