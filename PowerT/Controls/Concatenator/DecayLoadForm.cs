
// (c) 2024 Kazuki Kohzuki

using PowerT.Data;
using PowerT.Properties;
using System.Windows.Forms.DataVisualization.Charting;

namespace PowerT.Controls.Concatenator;

[DesignerCategory("Code")]
internal sealed class DecayLoadForm : Form
{
    private readonly TextBox tb_name;
    private readonly PathBox path_b, path_ab;
    private readonly Button btn_ok;

    private readonly CustomNumericUpDown nud_timeFrom, nud_timeTo, nud_signalFrom, nud_signalTo;
    private readonly CustomNumericUpDown nud_t0;
    private readonly Series series_t0;

    private readonly Chart _chart;
    private readonly Axis axisX, axisY;

    private Decay? _deacy;

    new internal string Name => this.tb_name.Text;

    internal Decay Decay { get; private set; } = Decay.Empty;

    internal DecayLoadForm()
    {
        this.Text = "Load decays";
        this.Size = new(1000, 700);
        this.MinimumSize = new(500, 400);
        this.Icon = Resources.Icon;

        var main_container = new SplitContainer()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            Panel1MinSize = 390,
            Panel2MinSize = 100,
            BorderStyle = BorderStyle.FixedSingle,
            Parent = this,
        };

        _ = new Label()
        {
            Text = "Name",
            Top = 10,
            Left = 10,
            Width = 40,
            Parent = main_container.Panel1,
        };

        this.tb_name = new TextBox()
        {
            Top = 10,
            Left = 50,
            Width = 300,
            Parent = main_container.Panel1,
        };

        _ = new Label()
        {
            Text = "a\u2212b signal",
            Top = 40,
            Left = 10,
            Width = 80,
            Parent = main_container.Panel1,
        };

        this.path_ab = new PathBox()
        {
            Top = 40,
            Left = 90,
            Width = 260,
            Parent = main_container.Panel1,
        };

        var browse_ab = new SignalSelectButton()
        {
            Title = "Select a\u2212b signal file",
            Target = this.path_ab,
            Top = 40,
            Left = 355,
            Width = 25,
            Parent = main_container.Panel1,
        };

        _ = new Label()
        {
            Text = "b signal",
            Top = 70,
            Left = 10,
            Width = 80,
            Parent = main_container.Panel1,
        };

        this.path_b = new PathBox()
        {
            Top = 70,
            Left = 90,
            Width = 260,
            Parent = main_container.Panel1,
        };

        var browse_b = new SignalSelectButton()
        {
            Title = "Select b signal file",
            Target = this.path_b,
            Top = 70,
            Left = 355,
            Width = 25,
            Parent = main_container.Panel1,
        };

        var load = new Button()
        {
            Text = "Load",
            Top = 100,
            Left = 40,
            Size = new(80, 40),
            Enabled = false,
            Parent = main_container.Panel1,
        };
        load.Click += LoadDecays;

        void EnableLoad(object? sender, EventArgs e)
            => load.Enabled = this.path_ab.TextLength * this.path_b.TextLength > 0;

        this.path_ab.TextChanged += EnableLoad;
        this.path_b.TextChanged += EnableLoad;

        #region chart

        this._chart = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderlineColor = Color.Black,
            BorderlineWidth = 2,
            BorderlineDashStyle = ChartDashStyle.Solid,
            SuppressExceptions = true,
            Parent = main_container.Panel2,
        };

        this.axisX = new Axis()
        {
            Title = "Time (us)",
            Minimum = 0,
            Maximum = 1000,
            LogarithmBase = 10,
            Interval = 1,
            LabelStyle = new() { Format = "f3" },
        };

        this.axisY = new Axis()
        {
            Title = "ΔuOD",
            Minimum = 0,
            Maximum = 10000,
            LogarithmBase = 10,
            Interval = 1,
            LabelStyle = new() { Format = "f1" },
        };

        this.axisX.MinorGrid.Enabled = this.axisY.MinorGrid.Enabled = true;
        this.axisX.MinorGrid.Interval = this.axisY.MinorGrid.Interval = 1;
        this.axisX.MinorGrid.LineColor = this.axisY.MinorGrid.LineColor = Color.LightGray;
        this.axisX.TitleFont = this.axisY.TitleFont = Program.AxisTitleFont;

        this._chart.ChartAreas.Add(new ChartArea()
        {
            AxisX = this.axisX,
            AxisY = this.axisY,
        });

        this.series_t0 = new Series()
        {
            ChartType = SeriesChartType.Line,
            BorderWidth = 3,
            BorderDashStyle = ChartDashStyle.DashDot,
            Color = Color.Green,
            LegendText = "t₀",
        };
        
        #endregion chart

        #region display range

        _ = new Label()
        {
            Text = "Time (us):",
            Location = new(10, 160),
            Size = new(60, 20),
            Parent = main_container.Panel1,
        };

        _ = new Label()
        {
            Text = "From",
            Location = new(70, 160),
            Size = new(40, 20),
            Parent = main_container.Panel1,
        };

        this.nud_timeFrom = new()
        {
            Location = new(110, 158),
            Size = new(80, 20),
            DecimalPlaces = 2,
            Minimum = 0M,
            Maximum = 1_000_000M,
            Value = (decimal)this.axisX.Minimum,
            Parent = main_container.Panel1,
        };
        this.nud_timeFrom.ValueChanged += SetDisplayRange;

        _ = new Label()
        {
            Text = "To",
            Location = new(200, 160),
            Size = new(20, 20),
            Parent = main_container.Panel1,
        };

        this.nud_timeTo = new()
        {
            Location = new(220, 158),
            Size = new(80, 20),
            DecimalPlaces = 2,
            Minimum = 0M,
            Maximum = 1_000_000M,
            Value = (decimal)this.axisX.Maximum,
            Parent = main_container.Panel1,
        };
        this.nud_timeTo.ValueChanged += SetDisplayRange;

        _ = new Label()
        {
            Text = "ΔuOD:",
            Location = new(10, 190),
            Size = new(60, 20),
            Parent = main_container.Panel1,
        };

        _ = new Label()
        {
            Text = "From",
            Location = new(70, 190),
            Size = new(40, 20),
            Parent = main_container.Panel1,
        };

        this.nud_signalFrom = new()
        {
            Location = new(110, 188),
            Size = new(80, 20),
            DecimalPlaces = 0,
            Minimum = -1_000M,
            Maximum = 1_000M,
            Value = (decimal)this.axisY.Minimum,
            Parent = main_container.Panel1,
        };
        this.nud_signalFrom.ValueChanged += SetDisplayRange;

        _ = new Label()
        {
            Text = "To",
            Location = new(200, 190),
            Size = new(20, 20),
            Parent = main_container.Panel1,
        };

        this.nud_signalTo = new()
        {
            Location = new(220, 188),
            Size = new(80, 20),
            DecimalPlaces = 0,
            Minimum = 0.001M,
            Maximum = 1_000_000M,
            Value = (decimal)this.axisY.Maximum,
            Parent = main_container.Panel1,
        };
        this.nud_signalTo.ValueChanged += SetDisplayRange;

        #endregion display range

        _ = new Label()
        {
            Text = "t₀ (us)",
            Top = 240,
            Left = 10,
            Width = 40,
            Parent = main_container.Panel1,
        };

        this.nud_t0 = new()
        {
            Top = 240,
            Left = 50,
            Width = 80,
            Minimum = 0,
            Maximum = 1_000_000,
            Value = 0,
            DecimalPlaces = 3,
            Parent = main_container.Panel1,
        };
        this.nud_t0.ValueChanged += Draw0;

        this.btn_ok = new Button()
        {
            Text = "OK",
            Top = 300,
            Left = 10,
            Size = new(80, 40),
            Enabled = false,
            Parent = main_container.Panel1,
        };
        this.btn_ok.Click += ClickOK;

        var cancel = new Button()
        {
            Text = "Cancel",
            Top = 300,
            Left = 100,
            Size = new(80, 40),
            DialogResult = DialogResult.Cancel,
            Parent = main_container.Panel1,
        };

        main_container.SplitterDistance = 390;
        main_container.Panel1.SizeChanged += (s, e) =>
        {
            this.tb_name.Width = main_container.Panel1.Width - 90;
            this.path_ab.Width = this.path_b.Width = main_container.Panel1.Width - 130;
            browse_ab.Left = browse_b.Left = main_container.Panel1.Width - 35;
        };
    } // ctor ()

    /// <summary>
    /// Initializes a new instance of the <see cref="DecayLoadForm"/> class
    /// with the specified data folder.
    /// </summary>
    /// <param name="folder">A path to the data folder.</param>
    /// <example>
    /// 100us
    /// ├─ 100us-a.csv
    /// ├─ 100us-b.csv
    /// ├─ 100us-a-b.csv
    /// └─ 100us-a-b-tdm.csv
    /// </example>
    internal DecayLoadForm(string folder) : this()
    {
        if (!Directory.Exists(folder)) return;

        var basename = Path.GetFileName(folder);

        var format_ab = Program.AMinusBSignalFormat;
        var format_b = Program.BSignalFormat;

        var name_ab = FileNameHandler.GetFileName(basename, format_ab);
        var name_b = FileNameHandler.GetFileName(basename, format_b);

        var file_ab = Path.Combine(folder, name_ab);
        var file_b = Path.Combine(folder, name_b);

        if (!File.Exists(file_ab) || !File.Exists(file_b)) return;

        this.tb_name.Text = basename;
        this.path_ab.Text = file_ab;
        this.path_b.Text = file_b;

        LoadDecays();
    } // ctor (string)

    private void LoadDecays(object? sender, EventArgs e)
        => LoadDecays();

    private void LoadDecays()
    {
        var file_ab = this.path_ab.Text;
        var file_b = this.path_b.Text;

        if (!File.Exists(file_ab))
        {
            MessageBox.Show(
                $"The file does not exist:\n{file_ab}",
                "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error
            );
            return;
        }

        if (!File.Exists(file_b))
        {
            MessageBox.Show(
                $"The file does not exist:\n{file_b}",
                "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error
            );
            return;
        }

        try
        {
            var decay_ab = Decay.FromFile(file_ab, 1e6, 1e6).Absolute;
            var decay_b = Decay.FromFile(file_b, 1e6, 1e6);

            this._deacy = decay_ab;

            // Time origin cannot be later than 25% of the b signal.
            // Reduce data points to improve performance.
            var timeMax = decay_b.TimeMax * 0.25;
            decay_ab = decay_ab.OfRange(0, timeMax);
            decay_b = decay_b.OfRange(0, timeMax);

            this._chart.Series.Clear();
            var series_ab = new Series()
            {
                ChartType = SeriesChartType.FastLine,
                BorderWidth = 2,
                Color = Color.Blue,
                LegendText = "a\u2212b",
            };
            var series_b = new Series()
            {
                ChartType = SeriesChartType.FastLine,
                BorderWidth = 2,
                Color = Color.Red,
                LegendText = "b",
            };

            series_ab.Points.AddDecay(decay_ab);
            series_b.Points.AddDecay(decay_b.Absolute);

            this._chart.Series.Add(series_ab);
            this._chart.Series.Add(series_b);

            var t0 = decay_b.MinBy(x => x.Signal).Time;

            this.nud_timeFrom.Value = (decimal)(t0 * 0.9);
            this.nud_timeTo.Value = (decimal)(t0 * 1.1);
            this.nud_signalFrom.Value = (decimal)0.0;
            this.nud_signalTo.Value = (decimal)(decay_b.Absolute.SignalMax * 1.2);

            var time_increment = (decimal)Math.Pow(10, Math.Ceiling(Math.Log10(decay_b.TimeMax)) - 2);
            this.nud_timeFrom.Increment = this.nud_timeFrom.ScrollIncrement = time_increment;
            this.nud_timeTo.Increment = this.nud_timeTo.ScrollIncrement = time_increment;
             
            var signal_increment = (decimal)Math.Pow(10, Math.Ceiling(Math.Log10(Math.Max(decay_ab.SignalMax, decay_b.SignalMax))) - 2);
            this.nud_signalFrom.Increment = this.nud_signalFrom.ScrollIncrement = signal_increment;

            this.nud_t0.Value = (decimal)t0;
            this.nud_t0.Increment = this.nud_t0.ScrollIncrement = (decimal)decay_b.TimeStep;
            this._chart.Series.Add(this.series_t0);
            Draw0();
        }
        catch (Exception e)
        {
            MessageBox.Show(
                $"Failed to load the decay data:\n{e.Message}",
                "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error
            );
            return;
        }

        this.btn_ok.Enabled = this.tb_name.TextLength > 0;
        this.tb_name.TextChanged += (s, e) => this.btn_ok.Enabled = this.tb_name.TextLength > 0;
    } // private void LoadDecays()

    private void SetDisplayRange(object? sender, EventArgs e)
    {
        var xInterval = Math.Pow(10, Math.Ceiling(Math.Log10(this.axisX.Maximum - this.axisX.Minimum)) - 2) * 2;
        this.axisX.Minimum = (double)this.nud_timeFrom.Value;
        this.axisX.Maximum = (double)this.nud_timeTo.Value;
        this.axisX.Interval = xInterval;
        this.axisX.MinorGrid.Interval = xInterval / 10;

        var yInterval = Math.Pow(10, Math.Ceiling(Math.Log10(this.axisY.Maximum - this.axisY.Minimum)) - 2);
        this.axisY.Minimum = (double)this.nud_signalFrom.Value;
        this.axisY.Maximum = (double)this.nud_signalTo.Value;
        this.axisY.Interval = yInterval;
        this.axisY.MinorGrid.Interval = yInterval / 10;
    } // private void SetDisplayRange (object?, EventArgs)

    private void Draw0()
    {
        var t0 = (double)this.nud_t0.Value;
        this.series_t0.Points.Clear();
        this.series_t0.Points.AddXY(t0, -1e6);
        this.series_t0.Points.AddXY(t0, 1e6);
    } // private void Draw0 ()

    private void Draw0(object? sender, EventArgs e)
        => Draw0();

    private void ClickOK(object? sender, EventArgs e)
    {
        if (this._deacy is null) return;

        var t0 = (double)this.nud_t0.Value;

        this.Decay = this._deacy.AddTime(-t0);
        this.DialogResult = DialogResult.OK;
        Close();
    } // private void ClickOK (object?, EventArgs)
} // internal sealed class DecayLoadForm : Form
