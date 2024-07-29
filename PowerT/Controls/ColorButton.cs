
// (c) 2024 Kazuki KOHZUKI

namespace PowerT.Controls;

/// <summary>
/// Represents a button that displays a color.
/// </summary>
[DesignerCategory("Code")]
internal class ColorButton : Button
{
    /// <summary>
    /// Gets the text of the button.
    /// </summary>
    new internal string Text
    {
        get => base.Text;
        private set
        {
            if (base.Text == value) return;

            base.Text = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets the foreground color of the button.
    /// </summary>
    new internal Color ForeColor
    {
        get => base.ForeColor;
        private set
        {
            if (base.ForeColor == value) return;

            base.ForeColor = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets the background color of the button.
    /// </summary>
    new internal Color BackColor
    {
        get => base.BackColor;
        private set
        {
            if (base.BackColor == value) return;

            base.BackColor = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the color of the button.
    /// </summary>
    internal Color Color
            {
        get => this.BackColor;
        set
        {
            if (this.Color == value) return;

            this.Text = GetColorText(value);
            this.BackColor = value;
            this.ForeColor = CalculateTextColor(value);
            Invalidate();
        }
    }

    /// <summary>
    /// Calculates the text color based on the specified color.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The calculated appropriate text color.</returns>
    protected virtual Color CalculateTextColor(Color color)
        => UIUtils.CalcInvertColor(color);

    /// <summary>
    /// Gets the string representation of the specified color.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The string representation of the color.</returns>
    protected virtual string GetColorText(Color color)
        => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
} // internal class ColorButton : Button
