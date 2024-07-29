
// (c) 2024 Kazuki KOHZUKI

namespace PowerT.Controls;

[DesignerCategory("Code")]
internal class ColorButton : Button
{
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

    protected virtual Color CalculateTextColor(Color color)
        => UIUtils.CalcInvertColor(color);

    protected virtual string GetColorText(Color color)
        => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
} // internal class ColorButton : Button
