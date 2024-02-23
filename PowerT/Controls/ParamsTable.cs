
// (c) 2024 Kazuki Kohzuki

using PowerT.Controls.Text;
using PowerT.Data;
using System.Windows.Forms.DataVisualization.Charting;

namespace PowerT.Controls;

[DesignerCategory("Code")]
internal sealed class ParamsTable : DataGridView
{
    private bool _syncAlpha, _syncTauT;

    /// <summary>
    /// Gets a collection of the rows as <see cref="ParamsRow"/>.
    /// </summary>
    internal IEnumerable<ParamsRow> ParamsRows
        => this.Rows.OfType<ParamsRow>();

    /// <summary>
    /// Gets or sets a value indicating whether to synchronize the alpha values.
    /// </summary>
    internal bool SyncAlpha
    {
        get => this._syncAlpha;
        set
        {
            this._syncAlpha = value;
            if (value) SetSameAlpha();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to synchronize the tauT values.
    /// </summary>
    internal bool SyncTauT
    {
        get => this._syncTauT;
        set
        {
            this._syncTauT = value;
            if (value) SetSameTauT();
        }
    }

    /// <summary>
    /// Gets the row by name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The row whose name is equal to the specified value.</returns>
    internal ParamsRow this[string name]
        => this.ParamsRows.First(row => row.Name == name);

    internal ParamsRow this[int index]
        => (ParamsRow)this.Rows[index];

    /// <summary>
    /// Initializes a new instance of the <see cref="ParamsTable"/> class.
    /// </summary>
    internal ParamsTable()
    {
        this.AllowUserToAddRows = false;
        this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.EnableHeadersVisualStyles = false;

        var col_show = new DataGridViewCheckBoxColumn()
        {
            HeaderText = "Show",
            DataPropertyName = "Show",
            ValueType = typeof(bool),
            TrueValue = true,
            FalseValue = false,
            Width = 40,
        };
        this.Columns.Add(col_show);  // 0

        var col_name = new DataGridViewTextBoxColumn()
        {
            HeaderText = "Name",
            DataPropertyName = "Name",
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        };
        this.Columns.Add(col_name);  // 1

        var col_A0 = new DataGridViewNumericBoxColumn(1000)
        {
            HeaderText = "A0",
            DataPropertyName = "A0",
            Width = 60,
        };
        this.Columns.Add(col_A0);  // 2
        CellPainting += PaintHandlerBuilder.CreateCellHandler(
            -1, 2,
            new("A", 0, 3, 4),
            new("0", 6, -1, 10)
        );

        var col_a = new DataGridViewNumericBoxColumn(1)
        {
            HeaderText = "a",
            DataPropertyName = "a",
            Width = 60,
        };
        this.Columns.Add(col_a);  // 3

        var col_alpha = new DataGridViewNumericBoxColumn(0.4)
        {
            HeaderText = "α",
            DataPropertyName = "alpha",
            Width = 60,
        };
        this.Columns.Add(col_alpha);  // 4

        var col_AT = new DataGridViewNumericBoxColumn(1000)
        {
            HeaderText = "At",
            DataPropertyName = "AT",
            Minimum = 0,
            Width = 60,
        };
        this.Columns.Add(col_AT);  // 5
        CellPainting += PaintHandlerBuilder.CreateCellHandler(
            -1, 5,
            new("A", 0, 3, 4),
            new("T", 6, -2, 10)
        );

        var col_tauT = new DataGridViewNumericBoxColumn(0.3)
        {
            HeaderText = "τt",
            DataPropertyName = "tauT",
            Width = 60,
        };
        this.Columns.Add(col_tauT);  // 6
        CellPainting += PaintHandlerBuilder.CreateCellHandler(
            -1, 6,
            new("τ", 0, 3, 4),
            new("T", 6, -3, 10)
        );

        var col_copy = new DataGridViewButtonColumn()
        {
            HeaderText = "Copy",
            Text = "Copy",
            UseColumnTextForButtonValue = true,
            Width = 60,
        };
        this.Columns.Add(col_copy);  // 7
    } // ctor ()

    override protected void OnCellValueChanged(DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex < 0 || e.RowIndex < 0)
            return;

        var row = this[e.RowIndex];

        if (e.ColumnIndex == 0) // Show
        {
            var series = row.ObservedSeries;
            var fitted = row.FittedSeries;

            series.Enabled = fitted.Enabled = row.Show;
        }

        if (e.ColumnIndex == 4 && this.SyncAlpha) // Alpha
        {
            var alpha = (double)row.Cells[4].Value;
            for (var i = 0; i < this.Rows.Count; i++)
            {
                if (i == e.RowIndex) continue;
                this.Rows[i].Cells[4].Value = alpha;
            }
            Invalidate();
        }
        else if (e.ColumnIndex == 6 && this.SyncTauT) // tauT
        {
            var tauT = (double)row.Cells[6].Value;
            for (var i = 0; i < this.Rows.Count; i++)
            {
                if (i == e.RowIndex) continue;
                this.Rows[i].Cells[6].Value = tauT;
            }
            Invalidate();
        }

        if (e.ColumnIndex is >= 2 and <= 6) // Params
        {
            var observed = row.ObservedSeries;
            var fitted = row.FittedSeries;

            fitted.Points.Clear();
            var f = row.Parameters.GetFunction();
            foreach (var time in row.Decay.Times)
                fitted.Points.AddXY(time, f(time));
            row.Cells[7].ToolTipText = row.Parameters.ToString();
        }

        base.OnCellValueChanged(e);
    } // override protected void OnCellValueChanged (DataGridViewCellEventArgs)

    override protected void OnCellContentClick(DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex < 0 || e.RowIndex < 0)
            return;

        var row = this[e.RowIndex];
        if (e.ColumnIndex == 0) // Show
        {
            ToggleShow(row);
        }
        else if (e.ColumnIndex == 7) // Copy
        {
            var eqn = row.Parameters.ToString();
            if (string.IsNullOrWhiteSpace(eqn)) return;
            Clipboard.SetText(eqn);
        }
        else
            base.OnCellContentClick(e);
    } // override protected void OnCellContentClick (DataGridViewCellEventArgs)

    override protected void OnCellContentDoubleClick(DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex < 0 || e.RowIndex < 0)
            return;

        if (e.ColumnIndex == 0) // Show
        {
            if (GetShownCount() == 1 && this[e.RowIndex].Show)
            {
                foreach (var row in this.ParamsRows)
                    row.Show = true;
            }
            else
            {
                this[e.RowIndex].Show = true;
                foreach ((var i, var row) in this.ParamsRows.Enumerate())
                    row.Show = i == e.RowIndex;
            }
            RefreshEdit();
            return;
        }

        base.OnCellContentDoubleClick(e);
    } // override protected void OnCellContentDoubleClick (DataGridViewCellEventArgs)

    private void ToggleShow(ParamsRow row)
    {
        if (!row.Show)
            row.Show = true;
        else
            row.Show = GetShownCount() <= 1;  // At least one data must be shown.
        RefreshEdit();
    } // private void ToggleShow (ParamsRow)

    private int GetShownCount()
        => this.ParamsRows.Count(r => r.Show);

    private void SetSameAlpha()
    {
        if (this.Rows.Count == 0) return;

        var alpha = (double)this.Rows[0].Cells[4].Value;
        for (var i = 1; i < this.Rows.Count; i++)
            this.Rows[i].Cells[4].Value = alpha;
        Invalidate();
    } // private void SetSameAlpha ()

    private void SetSameTauT()
    {
        if (this.Rows.Count == 0) return;

        var tauT = (double)this.Rows[0].Cells[6].Value;
        for (var i = 1; i < this.Rows.Count; i++)
            this.Rows[i].Cells[6].Value = tauT;
        Invalidate();
    } // private void SetSameTauT ()

    /// <summary>
    /// Add a new row to the table.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="decay">The decay.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="fitted">The fitted series.</param>
    /// <param name="observed">The observed series.</param>
    /// <returns>The added row.</returns>
    internal ParamsRow Add(string name, Decay decay, Parameters parameters, Series observed, Series fitted)
    {
        var row = new ParamsRow() {
            Name = name,
            Decay = decay,
            ObservedSeries = observed,
            FittedSeries = fitted
        };
        var index = this.Rows.Add(row);

        row = this[index];
        row.Cells[0].Value = true;
        row.Cells[1].Value = name;
        row.Parameters = parameters;
        row.Cells[7].ToolTipText = parameters.ToString();

        RefreshEdit();

        return row;
    } // internal ParamsRow Add (string, Decay, Parameters, Series, Series)
} // internal sealed class ParamsTable : DataGridView
