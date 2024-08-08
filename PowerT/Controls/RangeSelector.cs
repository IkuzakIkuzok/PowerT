
// (c) 2024 Kazuki Kohzuki

using Formatter = System.Func<decimal, string>;

namespace PowerT.Controls;

/// <summary>
/// Represents a range selector.
/// </summary>
internal class RangeSelector
{
    private readonly Label lb_main, lb_from, lb_to;
    private readonly LogarithmicNumericUpDown nud_from, nud_to;

    /// <summary>
    /// Gets or sets the text of the range selector.
    /// </summary>
    internal string Text
    {
        get => this.lb_main.Text;
        set => this.lb_main.Text = value;
    }

    /// <summary>
    /// Gets or sets the parent control.
    /// </summary>
    internal Control? Parent
    {
        get => this.lb_main.Parent;
        set
        {
            this.lb_main.Parent = this.lb_from.Parent = this.lb_to.Parent = value;
            this.nud_from.Parent = this.nud_to.Parent = value;
        }
    }

    /// <summary>
    /// Gets or sets the top position.
    /// </summary>
    internal int Top
    {
        get => this.lb_main.Top;
        set
        {
            this.lb_to.Top = this.lb_from.Top = this.lb_main.Top = value;
            this.nud_to.Top = this.nud_from.Top = value - 2;
        }
    }

    /// <summary>
    /// Gets or sets the left position.
    /// </summary>
    internal int Left
    {
        get => this.lb_main.Left;
        set
        {
            this.lb_main.Left = value;
            this.lb_from.Left = value + 60;
            this.lb_to.Left = value + 190;

            this.nud_from.Left = value + 100;
            this.nud_to.Left = value + 210;
        }
    }

    /// <summary>
    /// Gets or sets the start value of the range.
    /// </summary>
    internal decimal From
    {
        get => this.nud_from.Value;
        set => this.nud_from.Value = value;
    }

    /// <summary>
    /// Gets or sets the end value of the range.
    /// </summary>
    internal decimal To
    {
        get => this.nud_to.Value;
        set => this.nud_to.Value = value;
    }

    /// <summary>
    /// Gets or sets the decimal places of the start value.
    /// </summary>
    internal int FromDecimalPlaces
    {
        get => this.nud_from.DecimalPlaces;
        set => this.nud_from.DecimalPlaces = value;
    }

    /// <summary>
    /// Gets or sets the decimal places of the end value.
    /// </summary>
    internal int ToDecimalPlaces
    {
        get => this.nud_to.DecimalPlaces;
        set => this.nud_to.DecimalPlaces = value;
    }

    /// <summary>
    /// Gets or sets the minimum value of the start value.
    /// </summary>
    internal decimal FromMinimum
    {
        get => this.nud_from.Minimum;
        set => this.nud_from.Minimum = value;
    }

    /// <summary>
    /// Gets or sets the maximum value of the start value.
    /// </summary>
    internal decimal FromMaximum
    {
        get => this.nud_from.Maximum;
        set => this.nud_from.Maximum = value;
    }

    /// <summary>
    /// Gets or sets the minimum value of the end value.
    /// </summary>
    internal decimal ToMinimum
    {
        get => this.nud_to.Minimum;
        set => this.nud_to.Minimum = value;
    }

    /// <summary>
    /// Gets or sets the maximum value of the end value.
    /// </summary>
    internal decimal ToMaximum
    {
        get => this.nud_to.Maximum;
        set => this.nud_to.Maximum = value;
    }

    /// <summary>
    /// Gets or sets the formatter for the values.
    /// </summary>
    internal Formatter? Formatter
    {
        get => this.nud_from.Formatter;
        set
        {
            this.nud_from.Formatter = value;
            this.nud_to.Formatter = value;
        }
    }

    /// <summary>
    /// Occurs when the start value changes.
    /// </summary>
    internal event EventHandler? FromChanged;

    /// <summary>
    /// Occurs when the end value changes.
    /// </summary>
    internal event EventHandler? ToChanged;


    /// <summary>
    /// Initializes a new instance of the <see cref="RangeSelector"/> class.
    /// </summary>
    internal RangeSelector()
    {
        this.lb_main = new()
        {
            Size = new(60, 20),
        };

        this.lb_from = new()
        {
            Text = "From",
            Size = new(40, 20),
        };

        this.lb_to = new()
        {
            Text = "To",
            Size = new(20, 20),
        };

        this.nud_from = new()
        {
            Size = new(80, 20),
        };
        this.nud_from.ValueChanged += (s, e) => OnFromChanged(e);

        this.nud_to = new()
        {
            Size = new(80, 20),
        };
        this.nud_to.ValueChanged += (s, e) => OnToChanged(e);
    } // ctor ()

    /// <summary>
    /// Raises the <see cref="FromChanged"/> event.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnFromChanged(EventArgs e)
        => FromChanged?.Invoke(this, e);

    /// <summary>
    /// Raises the <see cref="ToChanged"/> event.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnToChanged(EventArgs e)
        => ToChanged?.Invoke(this, e);
} // internal class RangeSelector
