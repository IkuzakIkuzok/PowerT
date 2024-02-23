
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Controls.Text;

/// <summary>
/// Represents a chunk of text to be drawn.
/// </summary>
internal sealed class Chunk
{
    /// <summary>
    /// Gets the text content.
    /// </summary>
    internal string Text { get; }

    /// <summary>
    /// Gets the font size.
    /// </summary>
    internal float Size { get; }

    /// <summary>
    /// Gets the x-coordinate offset.
    /// </summary>
    internal int XOffset { get; }

    /// <summary>
    /// Gets the y-coordinate offset.
    /// </summary>
    internal int YOffset { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Chunk"/> class.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="size">The font size.</param>
    /// <param name="xOffset">The x offset.</param>
    /// <param name="yOffset">The y offset.</param>
    internal Chunk(string text, float size, int xOffset, int yOffset)
    {
        this.Text = text;
        this.Size = size;
        this.XOffset = xOffset;
        this.YOffset = yOffset;
    } // ctor (string, float, int, int)
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Chunk"/> class.
    /// </summary>
    /// <param name="text">The text.</param>
    internal Chunk(string text) : this(text, -1, 0, 0) { }
} // internal sealed class Chunk
