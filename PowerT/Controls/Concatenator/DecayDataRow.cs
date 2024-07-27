
// (c) 2024 Kazuki Kohzuki

using PowerT.Data;
using System.Windows.Forms.DataVisualization.Charting;

namespace PowerT.Controls.Concatenator;
internal sealed class DecayDataRow : DataGridViewRow
{
    /// <summary>
    /// Gets or sets the color of the row.
    /// </summary>
    internal Color Color
    {
        get => this.HeaderCell.Style.BackColor;
        set => this.HeaderCell.Style.BackColor = this.HeaderCell.Style.SelectionBackColor = value;
    }

    /// <summary>
    /// Gets the name of the row.
    /// </summary>
    internal string Name { get; }

    /// <summary>
    /// Gets the decay.
    /// </summary>
    internal Decay Decay { get; }

    /// <summary>
    /// Gets the used decay.
    /// </summary>
    internal Decay Used => this.Decay.OfRange(this.UseFrom, this.UseTo);

    /// <summary>
    /// Gets the series.
    /// </summary>
    internal required Series Series { get; init; }

    /// <summary>
    /// Gets the start time.
    /// </summary>
    internal double TimeStart { get; }

    /// <summary>
    /// Gets the end time.
    /// </summary>
    internal double TimeEnd { get; }

    private double GetCellValue(int index, double defaultValue)
        => this.Cells[index].Value is double value ? value : defaultValue;

    /// <summary>
    /// Gets or sets the time from which the decay is used.
    /// </summary>
    internal double UseFrom
    {
        get => GetCellValue(3, 0);
        set => this.Cells[3].Value = value;
    }

    /// <summary>
    /// Gets or sets the time to which the decay is used.
    /// </summary>
    internal double UseTo
    {
        get => GetCellValue(4, 0);
        set => this.Cells[4].Value = value;
    }

    /// <summary>
    /// Gets or sets the scaling factor.
    /// </summary>
    internal double Scaling
    {
        get => GetCellValue(5, 1);
        set => this.Cells[5].Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DecayDataRow"/> class.
    /// </summary>
    /// <param name="name">The name of the decay.</param>
    /// <param name="decay">The decay.</param>
    internal DecayDataRow(string name, Decay decay)
    {
        this.Name = name;
        this.Decay = decay;
        this.TimeStart = decay.TimeMin;
        this.TimeEnd = decay.TimeMax;
    } // ctor (string, Decay)
} // internal sealed class DecayDataRow : DataGridViewRow
