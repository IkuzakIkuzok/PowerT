
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
    private readonly Button btn_save;

    private readonly CheckBox cb_showGuide;
    private readonly LogarithmicNumericUpDown nud_guideIntercept;
    private readonly CustomNumericUpDown nud_guideSlope;
    private readonly ColorButton btn_guideColor;
    private readonly Series series_guide;

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
        this._decaysTable.UserDeletedRow += SetColor;
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

        _ = new DisplayRangeSelector(this.axisX, this.axisY)
        {
            Location = new(10, 30),
            Parent = this._decays_container.Panel2,
        };

        #region guide

        this.cb_showGuide = new()
        {
            Text = "Show guide line",
            Checked = true,
            Location = new(350, 90),
            Size = new(100, 20),
            Parent = this._decays_container.Panel2,
        };
        this.cb_showGuide.CheckedChanged += ToggleGuide;

        _ = new Label()
        {
            Text = "Intercept",
            Location = new(350, 30),
            Size = new(60, 20),
            Parent = this._decays_container.Panel2,
        };

        this.nud_guideIntercept = new()
        {
            Location = new(410, 28),
            Size = new(90, 20),
            Formatter = UIUtils.ExpFormatter,
            Minimum = 0.0001M,
            Maximum = 1_000_000M,
            Value = 100M,
            IncrementOrderBias = -1,
            Parent = this._decays_container.Panel2,
        };
        this.nud_guideIntercept.ValueChanged += UpdateGuide;

        _ = new Label()
        {
            Text = "Slope",
            Location = new(350, 60),
            Size = new(40, 20),
            Parent = this._decays_container.Panel2,
        };

        this.nud_guideSlope = new()
        {
            Location = new(410, 58),
            Size = new(90, 20),
            DecimalPlaces = 2,
            Minimum = 0.01M,
            Maximum = 1.00M,
            Value = 0.4M,
            Increment = 0.01M,
            ScrollIncrement = 0.01M,
            Parent = this._decays_container.Panel2,
        };
        this.nud_guideSlope.ValueChanged += UpdateGuide;

        this.btn_guideColor = new()
        {
            Location = new(460, 88),
            Size = new(80, 25),
            Parent = this._decays_container.Panel2,
        };
        this.btn_guideColor.Click += ChangeGuideColor;

        this.series_guide = new()
        {
            ChartType = SeriesChartType.Line,
            BorderDashStyle = ChartDashStyle.Dash,
            BorderWidth = 2,
            IsVisibleInLegend = false,
        };
        SetGuideColor();
        UpdateGuide();
        this._chart.Series.Add(this.series_guide);

        #endregion guide

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

        var clear = new Button()
        {
            Text = "Clear",
            Location = new(230, 100),
            Size = new(80, 40),
            Parent = this._decays_container.Panel2,
        };
        clear.Click += ClearData;

        Program.GradientChanged += SetColor;

        this._main_container.SplitterDistance = 400;
    } // ctor ()

    override protected void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Program.GradientChanged -= SetColor;
    } // override protected void OnClosed (EventArgs)

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
        var count = this._decaysTable.RowCount;
        if (count == 0) return;

        var gradient = new ColorGradient(Program.GradientStart, Program.GradientEnd, count);
        this._decaysTable.DecayDataRows.SetGradient(gradient);
    } // private void SetColor ()

    private void ToggleGuide(object? sender, EventArgs e)
    {
        this.nud_guideIntercept.Enabled =
        this.nud_guideSlope.Enabled =
        this.btn_guideColor.Enabled = this.cb_showGuide.Checked;

        if (this.cb_showGuide.Checked)
        {
            UpdateGuide();
            this._chart.Series.Add(this.series_guide);
        }
        else
        {
            this._chart.Series.Remove(this.series_guide);
        }
    } // private void ToggleGuide (object?, EventArgs)

    private void UpdateGuide(object? sender, EventArgs e)
        => UpdateGuide();

    private void UpdateGuide()
    {
        if (!this.cb_showGuide.Checked) return;

        var intercept = (double)this.nud_guideIntercept.Value;
        var slope = (double)this.nud_guideSlope.Value;
        this.series_guide.Points.Clear();
        void AddPoint(double x)
            => this.series_guide.Points.AddXY(x, intercept * Math.Pow(x, -slope));
        AddPoint(1e-6);
        AddPoint(1e6);
    } // private void UpdateGuide ()

    private void ChangeGuideColor(object? sender, EventArgs e)
    {
        using var cd = new ColorDialog()
        {
            Color = Program.GuideLineColor,
        };
        if (cd.ShowDialog() != DialogResult.OK) return;
        Program.GuideLineColor = cd.Color;
        SetGuideColor();
    } // private void ChangeGuideColor (object?, EventArgs)

    private void SetGuideColor()
    {
        var color = Program.GuideLineColor;
        this.btn_guideColor.Color = color;
        this.series_guide.Color = color;
    } // private void SetGuideColor ()

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

    private void ClearData(object? sender, EventArgs e)
    {
        if (this._decaysTable.RowCount == 0) return;

        var dr = MessageBox.Show(
            "Do you want to clear all decays?",
            "Warning",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Warning
        );
        if (dr != DialogResult.OK) return;

        foreach (var row in this._decaysTable.DecayDataRows)
            this._chart.Series.Remove(row.Series);
        this._decaysTable.Rows.Clear();
        this.btn_save.Enabled = false;
    } // private void ClearData (object?, EventArgs)
} // internal sealed class ConcatenateForm : Form
