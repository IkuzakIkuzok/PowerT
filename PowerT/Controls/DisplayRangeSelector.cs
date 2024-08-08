
// (c) 2024 Kazuki Kohzuki

using System.Windows.Forms.DataVisualization.Charting;

namespace PowerT.Controls;

/// <summary>
/// Represents a range selector for display.
/// </summary>
internal class DisplayRangeSelector
{
    private readonly RangeSelector time, signal;

    /// <summary>
    /// Gets the time range selector.
    /// </summary>
    internal RangeSelector Time => this.time;

    /// <summary>
    /// Gets the signal range selector.
    /// </summary>
    internal RangeSelector Signal => this.signal;

    /// <summary>
    /// Gets or sets the parent control.
    /// </summary>
    internal Control? Parent
    {
        get => this.time.Parent;
        set => this.time.Parent = this.signal.Parent = value;
    }

    /// <summary>
    /// Gets or sets the top position.
    /// </summary>
    internal int Top
    {
        get => this.time.Top;
        set
        {
            this.time.Top = value;
            this.signal.Top = value + 30;
        }
    }

    /// <summary>
    /// Gets or sets the left position.
    /// </summary>
    internal int Left
    {
        get => this.time.Left;
        set => this.time.Left = this.signal.Left = value;
    }

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    internal Point Location
    {
        get => new(this.Left, this.Top);
        set
        {
            this.Left = value.X;
            this.Top = value.Y;
        }
    }

    /// <summary>
    /// Gets or sets the time range.
    /// </summary>
    internal (double From, double To) TimeRange
    {
        get => ((double)this.time.From, (double)this.time.To);
        set
        {
            this.time.From = (decimal)value.From;
            this.time.To = (decimal)value.To;
        }
    }

    /// <summary>
    /// Gets or sets the signal range.
    /// </summary>
    internal (double From, double To) SignalRange
    {
        get => ((double)this.signal.From, (double)this.signal.To);
        set
        {
            this.signal.From = (decimal)value.From;
            this.signal.To = (decimal)value.To;
        }
    }

    /// <summary>
    /// Gets or sets the time axis.
    /// </summary>
    internal Axis? TimeAxis { get; set; }

    /// <summary>
    /// Gets or sets the signal axis.
    /// </summary>
    internal Axis? SignalAxis { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayRangeSelector"/> class.
    /// </summary>
    internal DisplayRangeSelector()
    {
        this.time = new()
        {
            Text = "Time (us):",
            Formatter = UIUtils.ExpFormatter,
            FromMinimum = 0.0001M,
            FromMaximum = 1_000M,
            FromDecimalPlaces = 2,
            ToMinimum = 5M,
            ToMaximum = 1_000_000M,
            ToDecimalPlaces = 0,
        };
        this.time.FromChanged += SetTimeRange;
        this.time.ToChanged += SetTimeRange;

        this.signal = new()
        {
            Text = "ΔuOD:",
            Formatter = UIUtils.ExpFormatter,
            FromMinimum = 0.001M,
            FromMaximum = 1_000M,
            FromDecimalPlaces = 2,
            ToMinimum = 50M,
            ToMaximum = 1_000_000M,
            ToDecimalPlaces = 0,
        };
        this.signal.FromChanged += SetSignalRange;
        this.signal.ToChanged += SetSignalRange;
    } // ctor ()

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayRangeSelector"/> class
    /// with the specified time and signal axes.
    /// </summary>
    /// <param name="timeAxis">The time axis.</param>
    /// <param name="signalAxis">The signal axis.</param>
    internal DisplayRangeSelector(Axis timeAxis, Axis signalAxis) : this()
    {
        this.time.From = (decimal)timeAxis.Minimum;
        this.time.To = (decimal)timeAxis.Maximum;
        this.signal.From = (decimal)signalAxis.Minimum;
        this.signal.To = (decimal)signalAxis.Maximum;

        this.TimeAxis = timeAxis;
        this.SignalAxis = signalAxis;
    } // ctor (Axis, Axis)

    protected virtual void SetTimeRange(object? sender, EventArgs e)
    {
        if (this.TimeAxis is null) return;
        this.TimeAxis.Minimum = (double)this.time.From;
        this.TimeAxis.Maximum = (double)this.time.To;
    } // protected virtual void SetTimeRange (object?, EventArgs)

    protected virtual void SetSignalRange(object? sender, EventArgs e)
    {
        if (this.SignalAxis is null) return;
        this.SignalAxis.Minimum = (double)this.signal.From;
        this.SignalAxis.Maximum = (double)this.signal.To;
    } // protected virtual void SetSignalRange (object?, EventArgs)
} // internal class DisplayRangeSelector
