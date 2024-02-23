
// (c) 2024 Kazuki Kohzuki

using System.Drawing.Drawing2D;
using SCBrush = System.Windows.Media.SolidColorBrush;
using WMBrush = System.Windows.Media.Brush;
using WMColor = System.Windows.Media.Color;

namespace PowerT.Controls;

/// <summary>
/// Represents a color gradient.
/// </summary>
internal class ColorGradient
{
    private static readonly Dictionary<nint, PaintEventHandler> paintHandlers = [];

    protected Color startColor, endColor;
    protected bool gammaCorrection = true;
    protected float gamma = 2.2f;

    protected readonly Color[] colors;

    /// <summary>
    /// Gets the color at the specified position.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <returns>The color at the specified position.</returns>
    internal ColorWrapper this[int position] => new(this.colors[position]);

    /// <summary>
    /// Gets the color at the specified relative position.
    /// </summary>
    /// <param name="position">The relative position.</param>
    /// <returns>The color at the specified relative position.</returns>
    internal ColorWrapper this[float position] => this[(int)(position * this.colors.Length)];

    /// <summary>
    /// Gets the width of the gradient.
    /// </summary>
    internal int Width => this.colors.Length;

    /// <summary>
    /// Gets or sets the start color.
    /// </summary>
    internal Color StartColor
    {
        get => this.startColor;
        set
        {
            if (this.startColor == value) return;
            this.startColor = value;
            SetColors();
        }
    }

    /// <summary>
    /// Gets or sets the end color.
    /// </summary>
    internal Color EndColor
    {
        get => this.endColor;
        set
        {
            if (this.endColor == value) return;
            this.endColor = value;
            SetColors();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use gamma correction.
    /// </summary>
    internal bool GammaCorrection
    {
        get => this.gammaCorrection;
        set
        {
            if (this.gammaCorrection == value) return;
            this.gammaCorrection = value;
            SetColors();
        }
    }

    /// <summary>
    /// Gets or sets the gamma value for correction.
    /// </summary>
    internal float Gamma
    {
        get => this.gamma;
        set
        {
            if (this.gamma == value) return;
            this.gamma = value;
            SetColors();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorGradient"/> class
    /// with the specified start and end colors.
    /// </summary>
    /// <param name="startColor">The start color.></param>
    /// <param name="endColor">The end color.</param>
    internal ColorGradient(Color startColor, Color endColor) : this(startColor, endColor, 100) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorGradient"/> class
    /// with the specified start and end colors and the width.
    /// </summary>
    /// <param name="startColor">The start color.</param>
    /// <param name="endColor">The end color.</param>
    /// <param name="width">The gradient width.</param>
    internal ColorGradient(Color startColor, Color endColor, int width)
    {
        this.colors = new Color[width];
        this.startColor = startColor;
        this.endColor = endColor;
        SetColors();
    } // internal ColorGradient(Color, Color, int, int)

    protected virtual void SetColors()
    {
        this.colors[0] = this.startColor;
        this.colors[^1] = this.endColor;
        var gamma = 1 / this.gamma;
        var rDiff = this.endColor.R - this.startColor.R;
        var gDiff = this.endColor.G - this.startColor.G;
        var bDiff = this.endColor.B - this.startColor.B;
        for (var i = 1; i < this.Width - 1; i++)
        {
            var coeff = (float)i / this.Width;
            var r = (int)(this.startColor.R + rDiff * coeff);
            var g = (int)(this.startColor.G + gDiff * coeff);
            var b = (int)(this.startColor.B + bDiff * coeff);
            if (this.gammaCorrection)
            {
                r = (int)(Math.Pow(r / 255f, gamma) * 255);
                g = (int)(Math.Pow(g / 255f, gamma) * 255);
                b = (int)(Math.Pow(b / 255f, gamma) * 255);
            }
            this.colors[i] = Color.FromArgb(r, g, b);
        }
    } // protected virtual void SetColors ()

    protected virtual Brush GetBrush(RectangleF rect, LinearGradientMode gradientMode)
        => new LinearGradientBrush(rect, this.startColor, this.endColor, gradientMode)
        {
            GammaCorrection = true,
        };

    /// <summary>
    /// Fills the specified control with the gradient.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="gradientMode">The gradient mode.</param>
    internal void FillRectangle(Control control, LinearGradientMode gradientMode)
    {
        if (paintHandlers.TryGetValue(control.Handle, out var oldHandler))
        {
            control.Paint -= oldHandler;
            paintHandlers.Remove(control.Handle);
        }

        var handler = new PaintEventHandler((object? sender, PaintEventArgs e) =>
        {
            var rect = new RectangleF(0, 0, control.Width, control.Height);
            e.Graphics.FillRectangle(GetBrush(rect, gradientMode), rect);
        });
        control.Paint += handler;
        paintHandlers.Add(control.Handle, handler);
        control.Refresh();
    } // internal void FillRectangle (Control, LinearGradientMode)

    /// <summary>
    /// Represents a color wrapper.
    /// </summary>
    internal class ColorWrapper
    {
        private readonly Color color;

        internal ColorWrapper(Color color)
        {
            this.color = color;
        } // ctor (Color)   

        public static implicit operator Color(ColorWrapper wrapper) => wrapper.color;

        public static implicit operator WMBrush(ColorWrapper wrapper) => new SCBrush(WMColor.FromArgb(wrapper.color.A, wrapper.color.R, wrapper.color.G, wrapper.color.B));
    } // internal class ColorWrapper
} // internal class ColorGradient
