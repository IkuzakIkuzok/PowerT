﻿
// (c) 2024 Kazuki Kohzuki

using PowerT.Data;
using System.Windows.Forms.DataVisualization.Charting;

namespace PowerT.Controls.Concatenator;

[DesignerCategory("Code")]
internal sealed class DecayDataTable : DataGridView
{
    private static readonly StringComparer _comparer = new();

    internal IEnumerable<DecayDataRow> DecayDataRows
        => this.Rows.OfType<DecayDataRow>();

    internal bool IsOrdered
    {
        get
        {
            for (var i = 1; i < this.Rows.Count; i++)
            {
                var row0 = (DecayDataRow)this.Rows[i - 1];
                var row1 = (DecayDataRow)this.Rows[i];

                if (row0.TimeEnd > row1.TimeEnd)
                    return false;
            }
            return true;
        }
    }

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

        var col_employStart = new DataGridViewNumericBoxColumn()
        {
            HeaderText = "Employ start (us)",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        };
        this.Columns.Add(col_employStart);  // 3

        var col_employEnd = new DataGridViewNumericBoxColumn()
        {
            HeaderText = "Employ end (us)",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        };
        this.Columns.Add(col_employEnd);  // 4

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

        var row = (DecayDataRow)this.Rows[e.RowIndex];
        var decay = row.Decay;
        var employStart = row.EmployStart;
        var employEnd = row.EmployEnd;

        if (e.ColumnIndex == 3)  // employ start
        {
            if (employStart < 0)
            {
                row.EmployStart = 0;
                return;
            }
            if (employStart > employEnd - decay.TimeStep * 2)
            {
                // At least one point must be included in any range with a width of 2 * TimeStep.
                row.EmployStart = employEnd - decay.TimeStep * 2;
                return;
            }
            
            var faster =
                GetFaster(decay).Where(row => row.Index != e.RowIndex)
                                .Where(row => row.EmployEnd > employStart);
            foreach (var r in faster)
                r.EmployEnd = employStart;
        }
        else if (e.ColumnIndex == 4)  // employ end
        {
            if (employEnd > decay.TimeMax)
            {
                row.EmployEnd = decay.TimeMax;
                return;
            }
            if (employEnd < employStart + decay.TimeStep * 2)
            {
                // At least one point must be included in any range with a width of 2 * TimeStep.
                row.EmployEnd = employStart + decay.TimeStep * 2;
                return;
            }
            
            var slower =
                GetSlower(decay).Where(row => row.Index != e.RowIndex)
                                .Where(row => row.EmployStart < employEnd);
            foreach (var r in slower)
                r.EmployStart = employEnd;
        }

        if (e.ColumnIndex is >= 3 and <= 5)
        {
            var series = row.Series;
            var scaling = row.Scaling;

            series.Points.Clear();
            foreach (var (time, signal) in row.EmployedDecay)
                series.Points.AddXY(time, signal * scaling);
        }

        base.OnCellValueChanged(e);
    } // override protected void OnCellValueChanged (e)

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

        var employStart =
            GetFaster(decay).Where(row => row.Index != index)
                                    .Select(row => row.EmployEnd)
                                    .Append(0.0)
                                    .Max();
        var employEnd = decay.Times.TakeLast(50).First();

        // At least one point must be included in any range with a width of 2 * TimeStep.
        if (employStart > employEnd - decay.TimeStep * 2)
            employStart = employEnd - decay.TimeStep * 2;

        row = (DecayDataRow)this.Rows[index];
        row.Cells[0].Value = name;
        row.Cells[1].Value = decay.TimeMin;
        row.Cells[2].Value = decay.TimeMax;

        /*
         * employ end must be set before employ start;
         * otherwise, bound adjusting runs recursively and stack overflow occurs
         * See OnCellValueChanged e.ColumnIndex == 3 or 4
         */
        row.Cells[4].Value = employEnd;
        row.Cells[3].Value = employStart;
        row.Cells[5].Value = 1.0;

        /*
         * Setting employ start and end triggers OnCellValueChanged
         * and the series points are already set in the event handler.
         */
        // foreach (var (time, signal) in decay.OfRange(employStart, employEnd))
        //     series.Points.AddXY(time, signal);

        RefreshEdit();

        return row;
    } // internal DecayDataRow Add (name, decay)
} // internal sealed class DecayDataTable : DataGridView
