
// (c) 2024 Kazuki Kohzuki

using LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode;

namespace PowerT.Controls;

[DesignerCategory("Code")]
internal class ColorGradientPicker : Form
{
    protected readonly ColorGradient colorGradient;
    private (Color, Color) returnColors;

    protected readonly ColorButton start, end;

    protected readonly Label lb_gradient;

    private readonly Button ok, cancel;

    internal ColorGradientPicker(Color startColor, Color endColor)
    {
        this.Text = "Color gradient";
        this.Size = this.MinimumSize = this.MaximumSize = new(330, 150);
        this.MaximizeBox = false;

        this.colorGradient = new(startColor, endColor);
        this.returnColors = (startColor, endColor);

        this.start = new()
        {
            Top = 20,
            Left = 20,
            Width = 80,
            TextAlign = ContentAlignment.MiddleCenter,
            FlatStyle = FlatStyle.Popup,
            Parent = this,
        };
        this.start.Click += SelectStartColor;

        this.end = new()
        {
            Top = 20,
            Left = 220,
            Width = 80,
            TextAlign = ContentAlignment.MiddleCenter,
            FlatStyle = FlatStyle.Popup,
            Parent = this,
        };
        this.end.Click += SelectEndColor;

        this.lb_gradient = new()
        {
            Top = 20,
            Left = 110,
            Width = 100,
            Parent = this,
        };

        this.ok = new()
        {
            Text = "OK",
            Top = 60,
            Left = 120,
            Size = new(80, 30),
            Parent = this,
        };
        this.ok.Click += (sender, e) =>
        {
            this.returnColors = (this.colorGradient.StartColor, this.colorGradient.EndColor);
            Close();
        };

        this.cancel = new()
        {
            Text = "Cancel",
            Top = 60,
            Left = 220,
            Size = new(80, 30),
            Parent = this,
        };
        this.cancel.Click += (sender, e) => Close();

        SetColor();
    } // ctor (Color, Color)

    internal ColorGradientPicker() : this(Color.Red, Color.Blue) { }

    new internal (Color, Color) ShowDialog()
    {
        base.ShowDialog();
        return this.returnColors;
    } // new internal (Color, Color) ShowDialog ()

    private static Color SelectColor(Color color)
    {
        using var cd = new ColorDialog()
        {
            Color = color,
        };
        return cd.ShowDialog() == DialogResult.OK ? cd.Color : color;
    } // private static Color SelectColor (Color)

    private void SelectStartColor(object? sender, EventArgs e)
    {
        this.colorGradient.StartColor = SelectColor(this.colorGradient.StartColor);
        SetColor();
    } // private void SelectStartColor (object?, EventArgs)

    private void SelectEndColor(object? sender, EventArgs e)
    {
        this.colorGradient.EndColor = SelectColor(this.colorGradient.EndColor);
        SetColor();
    } // private void SelectEndColor (object?, EventArgs)

    protected virtual void SetColor()
    {
        this.start.Color = this.colorGradient.StartColor;
        this.end.Color = this.colorGradient.EndColor;
        this.colorGradient.FillRectangle(this.lb_gradient, LinearGradientMode.Horizontal);
    } // protected virtual void SetColor ()
} // internal class ColorGradientPicker

