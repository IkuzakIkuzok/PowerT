
// (c) 2024 Kazuki Kohzuki

using System.Windows.Forms.DataVisualization.Charting;

namespace PowerT.Controls.Charting;

internal delegate void AxisRangeChangedEventHandler(object? sender, AxisRangeChangedEventArgs e);

internal sealed class AxisRangeChangedEventArgs : EventArgs
{
    internal double Minimum { get; init; }

    internal double Maximum { get; init; }

    internal AxisRangeChangedEventArgs(Axis axis)
    {
        this.Minimum = axis.Minimum;
        this.Maximum = axis.Maximum;
    } // internal AxisRangeChangedEventArgs (Axis)
} // internal sealed class AxisRangeChangedEventArgs : EventArgs
