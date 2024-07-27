
// (c) 2024 Kazuki Kohzuki

using PowerT.Properties;
using System.Windows.Forms.DataVisualization.Charting;

namespace PowerT.Controls.Concatenator;

[DesignerCategory("Code")]
internal sealed class ConcatenateForm : Form
{
    private readonly SplitContainer _main_container, _decays_container;
    private readonly Chart _chart;
    private readonly Axis axisX, axisY;
    private readonly DecayDataTable _decaysTable;
    private readonly LogarithmicNumericUpDown nud_timeFrom, nud_timeTo, nud_signalFrom, nud_signalTo;
    private readonly Button btn_save;

    internal ConcatenateForm()
    {
        this.Text = "Concatenate decays";
        this.Size = new Size(1000, 600);
        this.MinimumSize = new Size(400, 300);
        this.Icon = Resources.Icon;

        this._main_container = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 5,
            Panel1MinSize = 100,
            Panel2MinSize = 100,
            Parent = this,
        };

        #region chart

        this._chart = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderlineColor = Color.Black,
            BorderlineWidth = 2,
            BorderlineDashStyle = ChartDashStyle.Solid,
            SuppressExceptions = true,
            Parent = this._main_container.Panel1,
        };

        this.axisX = new Axis()
        {
            Title = "Time (us)",
            Minimum = 0.05,
            Maximum = 1000,
            LogarithmBase = 10,
            Interval = 1,
            LabelStyle = new() { Format = "e1" },
        };
        this.axisY = new Axis()
        {
            Title = "ΔuOD",
            Minimum = 1,
            Maximum = 10000,
            LogarithmBase = 10,
            Interval = 1,
            LabelStyle = new() { Format = "e1" },
        };
        this.axisX.MinorGrid.Enabled = this.axisY.MinorGrid.Enabled = true;
        this.axisX.MinorGrid.Interval = this.axisY.MinorGrid.Interval = 1;
        this.axisX.MinorGrid.LineColor = this.axisY.MinorGrid.LineColor = Color.LightGray;
        this.axisX.TitleFont = this.axisY.TitleFont = Program.AxisTitleFont;
        Program.AxisTitleFontChanged += (s, e) => this.axisX.TitleFont = this.axisY.TitleFont = Program.AxisTitleFont;

        this._chart.ChartAreas.Add(new ChartArea()
        {
            AxisX = this.axisX,
            AxisY = this.axisY,
        });

        var dummy = new Series()
        {
            ChartType = SeriesChartType.Point,
            IsVisibleInLegend = false,
            IsXValueIndexed = false,
        };
        dummy.Points.AddXY(1e-6, 1e-6);
        this._chart.Series.Add(dummy);

        this.axisX.IsLogarithmic = this.axisY.IsLogarithmic = true;
        this.axisX.LabelStyle.Font = this.axisY.LabelStyle.Font = Program.AxisLabelFont;

        #endregion chart

        this._decays_container = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 300,
            SplitterWidth = 5,
            Panel1MinSize = 100,
            Panel2MinSize = 100,
            Parent = this._main_container.Panel2,
        };

        this._decaysTable = new()
        {
            Dock = DockStyle.Fill,
            AllowDrop = true,
            AllowUserToDeleteRows = true,
            Parent = this._decays_container.Panel1,
        };
        this._decaysTable.Sorted += SetColor;
        this._decaysTable.UserDeletingRow += (sender, e) =>
        {
            if (e.Row is not DecayDataRow row) return;
            var series = row.Series;
            this._chart.Series.Remove(series);
        };
        this._decaysTable.DragEnter += (sender, e) =>
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) ?? false)
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        };
        this._decaysTable.DragDrop += (sender, e) =>
        {
            if (!(e.Data?.GetDataPresent(DataFormats.FileDrop) ?? false)) return;
            if (e.Data.GetData(DataFormats.FileDrop) is not string[] folders) return;
            Array.Sort(folders, new StringComparer());
            foreach (var folder in folders)
                AddDecay(folder);
        };

        #region display range

        _ = new Label()
        {
            Text = "Time (us): ",
            Location = new(10, 30),
            Size = new(60, 20),
            Parent = this._decays_container.Panel2,
        };

        _ = new Label()
        {
            Text = "From",
            Location = new(70, 30),
            Size = new(40, 20),
            Parent = this._decays_container.Panel2,
        };

        this.nud_timeFrom = new()
        {
            Location = new(110, 28),
            Size = new(60, 20),
            DecimalPlaces = 2,
            Minimum = 0.01M,
            Maximum = 10M,
            Value = (decimal)this.axisX.Minimum,
            Parent = this._decays_container.Panel2,
        };
        this.nud_timeFrom.ValueChanged += (sender, e) => this.axisX.Minimum = (double)this.nud_timeFrom.Value;

        _ = new Label()
        {
            Text = "To",
            Location = new(180, 30),
            Size = new(20, 20),
            Parent = this._decays_container.Panel2,
        };

        this.nud_timeTo = new()
        {
            Location = new(200, 28),
            Size = new(70, 20),
            DecimalPlaces = 0,
            Minimum = 50M,
            Maximum = 100_000M,
            Value = (decimal)this.axisX.Maximum,
            Parent = this._decays_container.Panel2,
        };
        this.nud_timeTo.ValueChanged += (sender, e) => this.axisX.Maximum = (double)this.nud_timeTo.Value;

        _ = new Label()
        {
            Text = "ΔuOD:",
            Location = new(10, 60),
            Size = new(60, 20),
            Parent = this._decays_container.Panel2,
        };

        _ = new Label()
        {
            Text = "From",
            Location = new(70, 60),
            Size = new(40, 20),
            Parent = this._decays_container.Panel2,
        };

        this.nud_signalFrom = new()
        {
            Location = new(110, 58),
            Size = new(60, 20),
            DecimalPlaces = 0,
            Minimum = 0.001M,
            Maximum = 1_000M,
            Value = (decimal)this.axisY.Minimum,
            Parent = this._decays_container.Panel2,
        };
        this.nud_signalFrom.ValueChanged += (sender, e) => this.axisY.Minimum = (double)this.nud_signalFrom.Value;

        _ = new Label()
        {
            Text = "To",
            Location = new(180, 60),
            Size = new(20, 20),
            Parent = this._decays_container.Panel2,
        };

        this.nud_signalTo = new()
        {
            Location = new(200, 58),
            Size = new(70, 20),
            DecimalPlaces = 0,
            Minimum = 50M,
            Maximum = 1_000_000M,
            Value = (decimal)this.axisY.Maximum,
            Parent = this._decays_container.Panel2,
        };
        this.nud_signalTo.ValueChanged += (sender, e) => this.axisY.Maximum = (double)this.nud_signalTo.Value;

        #endregion display range

        var add = new Button()
        {
            Text = "Add",
            Location = new(10, 100),
            Size = new(80, 40),
            Parent = this._decays_container.Panel2,
        };
        add.Click += AddDecay;

        this.btn_save = new()
        {
            Text = "Save",
            Location = new(120, 100),
            Size = new(80, 40),
            Enabled = false,
            Parent = this._decays_container.Panel2,
        };
        this.btn_save.Click += SaveToFile;

        this._main_container.SplitterDistance = 400;
    } // ctor ()

    private void AddDecay(object? sender, EventArgs e)
        => AddDecay();

    private void AddDecay(string folder = "")
    {
        var form = new DecayLoadForm(folder);
        if (form.ShowDialog() != DialogResult.OK) return;
        var decay = form.Decay;

        // Multiple rows with the same time range may cause an error.
        // Time end is used to identify the time range.
        if (this._decaysTable.DecayDataRows.Where(row => row.TimeEnd == decay.TimeMax).Any())
        {
            var dr = MessageBox.Show(
                "There is already a decay with the same end time.\nDo you want to add it anyway?",
                "Warning",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning
            );
            if (dr != DialogResult.OK) return;
        }

        var row = this._decaysTable.Add(form.Name, decay);
        SetColor();
        this._chart.Series.Add(row.Series);
        this.btn_save.Enabled = true;
    } // private void AddDecay ([string])

    private void SetColor(object? sender, EventArgs e)
        => SetColor();

    private void SetColor()
    {
        var gradient = new ColorGradient(Program.GradientStart, Program.GradientEnd, this._decaysTable.RowCount);
        foreach ((var i, var row) in this._decaysTable.DecayDataRows.Enumerate())
        {
            var color = gradient[i];
            row.Color = color;
            row.Series.Color = color;
        }
    } // private void SetColor ()

    private void SaveToFile(object? sender, EventArgs e)
        => SaveToFile();

    private void SaveToFile()
    {
        if (this._decaysTable.Rows.Count == 0) return;

        if (!this._decaysTable.IsOrdered)
        {
            var dr = MessageBox.Show(
                "The decays are not ordered by time.\nDo you want to order them before save?",
                "Warning",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning
            );

            if (dr == DialogResult.Cancel) return;
            if (dr == DialogResult.Yes)
            {
                // Sort by time end (index=2)
                this._decaysTable.Sort(this._decaysTable.Columns[2], ListSortDirection.Ascending);
            }
        }

        using var sfd = new SaveFileDialog()
        {
            Filter = "CSV files|*.csv|All files (*.*)|*.*",
            Title = "Save decays to file",
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        var filename = sfd.FileName;
        try
        {
            using var sw = new StreamWriter(sfd.FileName);
            foreach (var row in this._decaysTable.DecayDataRows)
            {
                var scaling = row.Scaling;
                foreach ((var time, var signal) in row.Used)
                    sw.WriteLine($"{time},{signal * scaling}");
            }
            

            FadingMessageBox.Show($"The decays have been saved to the file:\n{filename}", 0.8, 1000, 75, 0.1, this);
        }
        catch (Exception e)
        {
            MessageBox.Show($"An error occurred: {e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    } // private void SaveToFile ()
} // internal sealed class ConcatenateForm : Form
