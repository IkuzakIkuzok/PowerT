
// (c) 2024 Kazuki Kohzuki

using PowerT.Data;
using System.Windows.Forms.DataVisualization.Charting;

namespace PowerT.Controls.Concatenator;

[DesignerCategory("Code")]
internal sealed class DecayDataTable : DataGridView
{
    private static readonly StringComparer _comparer = new();

    /// <summary>
    /// Gets the decay data rows.
    /// </summary>
    internal IEnumerable<DecayDataRow> DecayDataRows
        => this.Rows.OfType<DecayDataRow>();

    /// <summary>
    /// Gets the decay data row by index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The row whose index is equal to the specified value.</returns>
    internal DecayDataRow this[int index]
        => (DecayDataRow)this.Rows[index];

    /// <summary>
    /// Gets a value indicating whether the decay data rows are ordered by time.
    /// </summary>
    internal bool IsOrdered
    {
        get
        {
            for (var i = 1; i < this.Rows.Count; i++)
            {
                var row0 = this[i - 1];
                var row1 = this[i];

                if (row0.TimeEnd > row1.TimeEnd)
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DecayDataTable"/> class.
    /// </summary>
    internal DecayDataTable()
    {
        this.AllowUserToAddRows = false;
        this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.EnableHeadersVisualStyles = false;

        var col_name = new DataGridViewTextBoxColumn()
        {
            HeaderText = "Name",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            ReadOnly = true,
        };
        this.Columns.Add(col_name);  // 0

        var col_timeStart = new DataGridViewTextBoxColumn()
        {
            HeaderText = "Time start (us)",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            ReadOnly = true,
        };
        this.Columns.Add(col_timeStart);  // 1

        var col_timeEnd = new DataGridViewTextBoxColumn()
        {
            HeaderText = "Time end (us)",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            ReadOnly = true,
        };
        this.Columns.Add(col_timeEnd);  // 2

        var col_useFrom = new DataGridViewNumericBoxColumn()
        {
            HeaderText = "Use from (us)",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        };
        this.Columns.Add(col_useFrom);  // 3

        var col_useTo = new DataGridViewNumericBoxColumn()
        {
            HeaderText = "Use to (us)",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        };
        this.Columns.Add(col_useTo);  // 4

        var col_scaling = new DataGridViewNumericBoxColumn(1.0)
        {
            HeaderText = "Scaling",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        };
        this.Columns.Add(col_scaling);  // 5
    } // ctor ()

    private IEnumerable<DecayDataRow> GetFaster(Decay decay)
        => this.DecayDataRows.Where(row => row.TimeEnd < decay.TimeMax);

    private IEnumerable<DecayDataRow> GetSlower(Decay decay)
        => this.DecayDataRows.Where(row => row.TimeEnd > decay.TimeMax);

    override protected void OnSortCompare(DataGridViewSortCompareEventArgs e)
    {
        e.Handled = true;

        if (e.Column.Index == 0)  // Name
        {
            e.SortResult = _comparer.Compare(e.CellValue1?.ToString(), e.CellValue2?.ToString());
        }
        else
        {
            if (e.CellValue1 is not double d1 || e.CellValue2 is not double d2) return;
            e.SortResult = d1.CompareTo(d2);
        }

        base.OnSortCompare(e);
    } // override protected void OnSortCompare (e)

    override protected void OnCellValueChanged(DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex < 0 || e.RowIndex < 0)
            return;

        var row = this[e.RowIndex];
        var decay = row.Decay;
        var useFrom = row.UseFrom;
        var useTo = row.UseTo;

        if (e.ColumnIndex == 3)  // use from
        {
            if (useFrom < 0)
            {
                row.UseFrom = 0;
                return;
            }
            if (useFrom > useTo - decay.TimeStep * 2)
            {
                // At least one point must be included in any range with a width of 2 * TimeStep.
                row.UseFrom = useTo - decay.TimeStep * 2;
                return;
            }
            
            var faster =
                GetFaster(decay).Where(row => row.Index != e.RowIndex)
                                .Where(row => row.UseTo > useFrom);
            foreach (var r in faster)
                r.UseTo = useFrom;
        }
        else if (e.ColumnIndex == 4)  // use to
        {
            if (useTo > decay.TimeMax)
            {
                row.UseTo = decay.TimeMax;
                return;
            }
            if (useTo < useFrom + decay.TimeStep * 2)
            {
                // At least one point must be included in any range with a width of 2 * TimeStep.
                row.UseTo = useFrom + decay.TimeStep * 2;
                return;
            }
            
            var slower =
                GetSlower(decay).Where(row => row.Index != e.RowIndex)
                                .Where(row => row.UseFrom < useTo);
            foreach (var r in slower)
                r.UseFrom = useTo;
        }

        if (e.ColumnIndex is >= 3 and <= 5)
        {
            var series = row.Series;
            var scaling = row.Scaling;

            series.Points.Clear();
            foreach (var (time, signal) in row.Used * scaling)
                series.Points.AddXY(time, signal);
        }

        base.OnCellValueChanged(e);
    } // override protected void OnCellValueChanged (e)

    /// <summary>
    /// Adds a decay data row.
    /// </summary>
    /// <param name="name">The name of the dacay.</param>
    /// <param name="decay">The dacay.</param>
    /// <returns>The new row.</returns>
    internal DecayDataRow Add(string name, Decay decay)
    {
        var series = new Series(name)
        {
            ChartType = SeriesChartType.Line,
            BorderWidth = 2,
        };

        var row = new DecayDataRow(name, decay)
        {
            Series = series
        };
        var index = this.Rows.Add(row);

        var useFrom =
            GetFaster(decay).Where(row => row.Index != index)
                            .Select(row => row.UseTo)
                            .Append(0.0)
                            .Max();
        var useTo = decay.Times.TakeLast(50).First();

        // At least one point must be included in any range with a width of 2 * TimeStep.
        if (useFrom > useTo - decay.TimeStep * 2)
            useFrom = useTo - decay.TimeStep * 2;

        row = this[index];
        row.Cells[0].Value = name;
        row.Cells[1].Value = decay.TimeMin;
        row.Cells[2].Value = decay.TimeMax;

        /*
         * useTo must be set before useFrom;
         * otherwise, bound adjusting runs recursively and stack overflow occurs
         * See OnCellValueChanged e.ColumnIndex == 3 or 4
         */
        row.Cells[4].Value = useTo;
        row.Cells[3].Value = useFrom;
        row.Cells[5].Value = 1.0;

        /*
         * Setting useFrom and useTo triggers OnCellValueChanged
         * and the series points are already set in the event handler.
         */
        // foreach (var (time, signal) in decay.OfRange(useFrom, useTo))
        //     series.Points.AddXY(time, signal);

        RefreshEdit();

        return row;
    } // internal DecayDataRow Add (name, decay)
} // internal sealed class DecayDataTable : DataGridView
