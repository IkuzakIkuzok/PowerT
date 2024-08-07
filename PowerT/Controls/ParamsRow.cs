
// (c) 2024 Kazuki Kohzuki

using PowerT.Data;
using System.Windows.Forms.DataVisualization.Charting;

namespace PowerT.Controls;

/// <summary>
/// Represents a row in the parameters table.
/// </summary>
internal class ParamsRow : DataGridViewRow, IColor
{
    /// <summary>
    /// Gets or sets the color of the row.
    /// </summary>
    public Color Color
    {
        get => this.HeaderCell.Style.BackColor;
        set
        {
            this.HeaderCell.Style.BackColor = this.HeaderCell.Style.SelectionBackColor = value;
            this.ObservedSeries.Color = this.FitSeries.Color = value;
        }
    }

    /// <summary>
    /// Gets the name of the row.
    /// </summary>
    internal required string Name { get; init; }

    /// <summary>
    /// Gets the decay.
    /// </summary>
    internal required Decay Decay { get; init; }

    /// <summary>
    /// Gets or sets the series representing the observed data.
    /// </summary>
    internal required Series ObservedSeries { get; init; }

    /// <summary>
    /// Gets or sets the series representing the fit data.
    /// </summary>
    internal required Series FitSeries { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to show data on the plot.
    /// </summary>
    internal bool Show
    {
        get => (bool)(this.Cells[0].Value ?? true);
        set => this.Cells[0].Value = value;
    }

    private double GetCellValue(int index, double defaultValue) =>
        this.Cells[index].Value is double value ? value : defaultValue;

    /// <summary>
    /// Gets or sets the A0 value.
    /// </summary>
    internal double A0
    {
        get => GetCellValue(2, 1000);
        set => this.Cells[2].Value = value;
    }

    /// <summary>
    /// Gets or sets the A value.
    /// </summary>
    internal double A
    {
        get => GetCellValue(3, 1);
        set => this.Cells[3].Value = value;
    }

    /// <summary>
    /// Gets or sets the alpha value.
    /// </summary>
    internal double Alpha
    {
        get => GetCellValue(4, 0.4);
        set => this.Cells[4].Value = value;
    }

    /// <summary>
    /// Gets or sets the AT value.
    /// </summary>
    internal double AT
    {
        get => GetCellValue(5, 1000);
        set => this.Cells[5].Value = value;
    }

    /// <summary>
    /// Gets or sets the TauT value.
    /// </summary>
    internal double TauT
    {
        get => GetCellValue(6, 0.3);
        set => this.Cells[6].Value = value;
    }

    /// <summary>
    /// Gets or sets the parameters.
    /// </summary>
    internal Parameters Parameters
    {
        get => new(this.A0, this.A, this.Alpha, this.AT, this.TauT);
        set
        {
            this.A0 = value.A0;
            this.A = value.A;
            this.Alpha = value.Alpha;
            this.AT = value.AT;
            this.TauT = value.TauT;
        }
    }
} // internal class ParamsRow : DataGridViewRow, IColor
