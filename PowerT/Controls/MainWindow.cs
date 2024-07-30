
// (c) 2024 Kazuki Kohzuki

using PowerT.Controls.Concatenator;
using PowerT.Controls.Text;
using PowerT.Data;
using PowerT.Properties;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

namespace PowerT.Controls;

[DesignerCategory("Code")]
internal sealed partial class MainWindow : Form
{
    private readonly TextBox tb_sources;
    private readonly SplitContainer _container, _params_container;
    private readonly Chart _chart;
    private readonly Axis axisX, axisY;
    private readonly ParamsTable _paramsTable;
    private readonly CheckBox cb_syncAlpha, cb_syncTauT;
    private readonly LogarithmicNumericUpDown nud_timeFrom, nud_timeTo, nud_signalFrom, nud_signalTo;
    private readonly ToolStripMenuItem m_showObserved, m_showFitted;
    private readonly ToolStripMenuItem m_savePlot, m_copy, m_paste, m_clearBeforeLoad;

    private readonly List<(string, Decay)> _decays = [];

    private ChartDashStyle fitted_style = ChartDashStyle.Solid;

    internal MainWindow()
    {
        this.Text = "Power T";
        this.Size = new(900, 530);
        this.MinimumSize = new(300, 200);
        this.Icon = Resources.Icon;

        #region sources

        _ = new Label()
        {
            Text = "Sources",
            Location = new(10, 40),
            Size = new(60, 20),
            Parent = this,
        };

        this.tb_sources = new()
        {
            Location = new(70, 40),
            Size = new(660, 20),
            AllowDrop = true,
            Parent = this,
        };
        this.tb_sources.DragEnter += SourcesDragEnter;
        this.tb_sources.DragDrop += SourcesDragDrop;

        var btn_browse = new Button()
        {
            Text = "Browse",
            Location = new(740, 39),
            Size = new(60, 25),
            Parent = this,
        };
        btn_browse.Click += BrowseSources;

        var btn_load = new Button()
        {
            Text = "Load",
            Location = new(810, 39),
            Size = new(60, 25),
            Enabled = false,
            Parent = this,
        };
        btn_load.Click += LoadSources;
        this.tb_sources.TextChanged += (sender, e) => btn_load.Enabled = !string.IsNullOrWhiteSpace(this.tb_sources.Text);

        #endregion sources

        this._container = new()
        {
            Location = new(10, 80),
            Size = new(860, 400),
            Orientation = Orientation.Vertical,
            SplitterDistance = 450,
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
            Parent = this._container.Panel1,
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
            Minimum = 10,
            Maximum = 10000,
            LogarithmBase = 10,
            Interval = 1,
            LabelStyle = new() { Format = "e1" },
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

        this._params_container = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 300,
            Parent = this._container.Panel2,
        };

        this._paramsTable = new()
        {
            Dock = DockStyle.Fill,
            SyncAlpha = true,
            SyncTauT = true,
            Parent = this._params_container.Panel1,
        };
        this._paramsTable.Sorted += SetColor;
        this._paramsTable.RowMoved += SetColor;

        this.cb_syncAlpha = new()
        {
            Text = "Sync α",
            Checked = true,
            Location = new(10, 10),
            Width = 80,
            Parent = this._params_container.Panel2,
        };
        this.cb_syncAlpha.CheckedChanged += (sender, e) => this._paramsTable.SyncAlpha = this.cb_syncAlpha.Checked;

        this.cb_syncTauT = new()
        {
            //Text = "Sync τt",
            Checked = true,
            Location = new(100, 10),
            Width = 80,
            Parent = this._params_container.Panel2,
        };
        this.cb_syncTauT.Paint += PaintHandlerBuilder.Create( 
            new("Sync τ", 0, 18, 3),
            new("T", 6, -3, 10)
        );
        this.cb_syncTauT.CheckedChanged += (sender, e) => this._paramsTable.SyncTauT = this.cb_syncTauT.Checked;

        #region display range

        _ = new Label()
        {
            Text = "Time (us):",
            Location = new(10, 60),
            Size = new(60, 20),
            Parent = this._params_container.Panel2,
        };

        _ = new Label()
        {
            Text = "From",
            Location = new(70, 60),
            Size = new(40, 20),
            Parent = this._params_container.Panel2,
        };

        this.nud_timeFrom = new()
        {
            Location = new(110, 58),
            Size = new(80, 20),
            DecimalPlaces = 2,
            Minimum = 0.001M,
            Maximum = 1_000M,
            Value = (decimal)this.axisX.Minimum,
            Formatter = UIUtils.ExpFormatter,
            Parent = this._params_container.Panel2,
        };
        this.nud_timeFrom.ValueChanged += (sender, e) => this.axisX.Minimum = (double)this.nud_timeFrom.Value;

        _ = new Label()
        {
            Text = "To",
            Location = new(200, 60),
            Size = new(20, 20),
            Parent = this._params_container.Panel2,
        };

        this.nud_timeTo = new()
        {
            Location = new(220, 58),
            Size = new(80, 20),
            DecimalPlaces = 0,
            Minimum = 5M,
            Maximum = 1_000_000M,
            Value = (decimal)this.axisX.Maximum,
            Formatter = UIUtils.ExpFormatter,
            Parent = this._params_container.Panel2,
        };
        this.nud_timeTo.ValueChanged += (sender, e) => this.axisX.Maximum = (double)this.nud_timeTo.Value;

        _ = new Label()
        {
            Text = "ΔuOD:",
            Location = new(10, 90),
            Size = new(60, 20),
            Parent = this._params_container.Panel2,
        };

        _ = new Label()
        {
            Text = "From",
            Location = new(70, 90),
            Size = new(40, 20),
            Parent = this._params_container.Panel2,
        };

        this.nud_signalFrom = new()
        {
            Location = new(110, 88),
            Size = new(80, 20),
            DecimalPlaces = 0,
            Minimum = 0.001M,
            Maximum = 1_000M,
            Value = (decimal)this.axisY.Minimum,
            Formatter = UIUtils.ExpFormatter,
            Parent = this._params_container.Panel2,
        };
        this.nud_signalFrom.ValueChanged += (sender, e) => this.axisY.Minimum = (double)this.nud_signalFrom.Value;

        _ = new Label()
        {
            Text = "To",
            Location = new(200, 90),
            Size = new(20, 20),
            Parent = this._params_container.Panel2,
        };

        this.nud_signalTo = new()
        {
            Location = new(220, 88),
            Size = new(80, 20),
            DecimalPlaces = 0,
            Minimum = 50M,
            Maximum = 1_000_000M,
            Value = (decimal)this.axisY.Maximum,
            Formatter = UIUtils.ExpFormatter,
            Parent = this._params_container.Panel2,
        };
        this.nud_signalTo.ValueChanged += (sender, e) => this.axisY.Maximum = (double)this.nud_signalTo.Value;

        #endregion display range

        #region menu

        var ms = new MenuStrip()
        {
            Parent = this,
        };
        this.MainMenuStrip = ms;

        #region menu.file

        var m_file = new ToolStripMenuItem()
        {
            Text = "&File",
        };
        ms.Items.Add(m_file);

        var m_open = new ToolStripMenuItem()
        {
            Text = "&Open",
            ShortcutKeys = Keys.Control | Keys.O,
        };
        m_open.Click += OpenSources;
        m_file.DropDownItems.Add(m_open);

        this.m_savePlot = new ToolStripMenuItem()
        {
            Text = "&Save Plot",
            ShortcutKeys = Keys.Control | Keys.S,
            Enabled = false,
        };
        this.m_savePlot.Click += SevePlot;
        m_file.DropDownItems.Add(this.m_savePlot);

        m_file.DropDownItems.Add(new ToolStripSeparator());

        var m_exit = new ToolStripMenuItem()
        {
            Text = "E&xit",
            ShortcutKeys = Keys.Alt | Keys.F4,
        };
        m_exit.Click += (sender, e) => Close();
        m_file.DropDownItems.Add(m_exit);

        #endregion menu.file

        #region menu.view

        var m_view = new ToolStripMenuItem()
        {
            Text = "&View"
        };
        ms.Items.Add(m_view);

        var m_colorGradient = new ToolStripMenuItem()
        {
            Text = "Color &Gradient"
        };
        m_colorGradient.Click += SelectColorGradient;
        m_view.DropDownItems.Add(m_colorGradient);

        var m_fitted = new ToolStripMenuItem()
        {
            Text = "&Fitted Curves"
        };
        m_view.DropDownItems.Add(m_fitted);

        #region fitted.width

        var m_fitted_width = new ToolStripMenuItem()
        {
            Text = "&Width"
        };
        m_fitted.DropDownItems.Add(m_fitted_width);

        int[] widths = [1, 2, 3, 4, 6, 8, 10];
        foreach (var w in widths)
        {
            var item = new ToolStripMenuItem()
            {
                Text = w.ToString(),
                Tag = w,
            };
            item.Click += (sender, e) =>
            {
                Program.FittedWidth = w;
                foreach (var row in this._paramsTable.ParamsRows)
                {
                    var s = row.FittedSeries;
                    if (s is null) continue;
                    s.BorderWidth = w;
                }
            };
            m_fitted_width.DropDownItems.Add(item);
        }
        m_fitted_width.DropDownOpening += (sender, e) =>
        {
            foreach (ToolStripMenuItem item in m_fitted_width.DropDownItems)
                item.Checked = (int)item.Tag! == Program.FittedWidth;
        };

        #endregion fitted.width

        #region fitted.style

        var m_fitted_style = new ToolStripMenuItem()
        {
            Text = "&Style"
        };
        m_fitted.DropDownItems.Add(m_fitted_style);

        var styles = new[] { ChartDashStyle.Solid, ChartDashStyle.Dash, ChartDashStyle.Dot, ChartDashStyle.DashDot, ChartDashStyle.DashDotDot };
        foreach (var style in styles)
        {
            var item = new ToolStripMenuItem()
            {
                Text = style.ToString(),
                Tag = style,
            };
            item.Click += (sender, e) =>
            {
                this.fitted_style = style;
                foreach (var row in this._paramsTable.ParamsRows)
                {
                    var s = row.FittedSeries;
                    if (s is null) continue;
                    s.BorderDashStyle = style;
                }
            };
            m_fitted_style.DropDownItems.Add(item);
        }
        m_fitted_style.DropDownOpening += (sender, e) =>
        {
            foreach (ToolStripMenuItem item in m_fitted_style.DropDownItems)
                item.Checked = (ChartDashStyle)item.Tag! == this.fitted_style;
        };

        #endregion fitted.style

        var m_font = new ToolStripMenuItem()
        {
            Text = "&Font"
        };
        m_view.DropDownItems.Add(m_font);

        var m_axis_label = new ToolStripMenuItem()
        {
            Text = "&Axis Label"
        };
        m_font.DropDownItems.Add(m_axis_label);
        m_axis_label.Click += SelectAxisLabelFont;

        var m_axis_title = new ToolStripMenuItem()
        {
            Text = "&Axis Title"
        };
        m_font.DropDownItems.Add(m_axis_title);
        m_axis_title.Click += SelectAxisTitleFont;

        #endregion menu.view

        #region menu.data

        var m_data = new ToolStripMenuItem()
        {
            Text = "&Data",
        };
        ms.Items.Add(m_data);

        this.m_showObserved = new()
        {
            Text = "&Observed",
            Checked = true,
            ShortcutKeys = Keys.Control | Keys.R,
        };
        this.m_showObserved.Click += ToggleOberserved;
        m_data.DropDownItems.Add(this.m_showObserved);

        this.m_showFitted = new()
        {
            Text = "&Fitted",
            Checked = true,
            ShortcutKeys = Keys.Control | Keys.F,
        };
        this.m_showFitted.Click += ToggleFitted;
        m_data.DropDownItems.Add(this.m_showFitted);

        m_data.DropDownItems.Add(new ToolStripSeparator());

        var m_reestimate = new ToolStripMenuItem()
        {
            Text = "&Re-estimate",
        };
        m_reestimate.Click += (sender, e) =>
        {
            foreach (var row in this._paramsTable.ParamsRows)
                row.Parameters = row.Decay.EstimateParams();
        };
        m_data.DropDownItems.Add(m_reestimate);

        this.m_copy = new ToolStripMenuItem()
        {
            Text = "&Copy table",
            ShortcutKeys = Keys.Control | Keys.C,
            Enabled = false,
        };
        this.m_copy.Click += CopyToClipboard;
        m_data.DropDownItems.Add(this.m_copy);

        this.m_paste = new ToolStripMenuItem()
        {
            Text = "&Paste table",
            ShortcutKeys = Keys.Control | Keys.V,
            Enabled = false,
        };
        this.m_paste.Click += PasteFromClipboard;
        m_data.DropDownItems.Add(this.m_paste);

        m_data.DropDownItems.Add(new ToolStripSeparator());

        this.m_clearBeforeLoad = new()
        {
            Text = "&Clear before Load",
            Checked = true,
        };
        this.m_clearBeforeLoad.Click += (sender, e) => this.m_clearBeforeLoad.Checked = !this.m_clearBeforeLoad.Checked;
        m_data.DropDownItems.Add(this.m_clearBeforeLoad);

        #endregion menu.data

        #region menu.tools

        var m_tools = new ToolStripMenuItem()
        {
            Text = "&Tools",
        };
        ms.Items.Add(m_tools);

        var m_concatenate = new ToolStripMenuItem()
        {
            Text = "&Concatenate decays",
        };
        m_concatenate.Click += (sender, e) => new ConcatenateForm().Show();
        m_tools.DropDownItems.Add(m_concatenate);

        var m_filenameFormat = new ToolStripMenuItem()
        {
            Text = "&Filename Format",
        };
        m_filenameFormat.Click += EditFilenameFormat;
        m_tools.DropDownItems.Add(m_filenameFormat);

        #endregion menu.tools

        #region menu.help

        var m_help = new ToolStripMenuItem()
        {
            Text = "&Help",
        };
        ms.Items.Add(m_help);

        var m_guthub = new ToolStripMenuItem()
        {
            Text = "Open &GitHub repository",
        };
        m_guthub.Click += (sender, e) => Process.Start("explorer", Program.GITHUB_REPOSITORY);
        m_help.DropDownItems.Add(m_guthub);

        #endregion menu.help

        #endregion menu

        SizeChanged += (sender, e) =>
        {
            this.tb_sources.Width = this.Width - 240;
            btn_browse.Left = this.Width - 160;
            btn_load.Left = this.Width - 90;
            this._container.Width = this.Width - 40;
            this._container.Height = this.Height - 130;
        };
    } // ctor ()

    private void SourcesDragEnter(object? sender, DragEventArgs e)
    {
        if (!(e.Data?.GetDataPresent(DataFormats.FileDrop) ?? false)) return;
        e.Effect = DragDropEffects.Copy;
    } // void SourcesDragEnter (object?, DragEventArgs)

    private void SourcesDragDrop(object? sender, DragEventArgs e)
    {
        if (!(e.Data?.GetDataPresent(DataFormats.FileDrop) ?? false)) return;
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files) return;
        this.tb_sources.Text = string.Join(';', files);
    } // private void SourcesDragDrop (object?, DragEventArgs)

    private void BrowseSources(object? sender, EventArgs e)
        => BrowseSources();

    private bool BrowseSources()
    {
        using var ofd = new OpenFileDialog()
        {
            Title = "Select sources",
            Filter = "CSV files|*.csv|All files|*.*",
            Multiselect = true,
        };
        if (ofd.ShowDialog() != DialogResult.OK) return false;
        this.tb_sources.Text = string.Join(';', ofd.FileNames);
        return true;
    } // private bool BrowseSources ()

    private void LoadSources(object? sender, EventArgs e)
        => LoadSources();

    private void LoadSources()
    {
        try
        {
            var sources = this.tb_sources.Text.Split(';');
            var decays
                = sources.Where(File.Exists)
                         .Select(f => (Path.GetFileNameWithoutExtension(f), Decay.FromFile(f)));
            if (!decays.Any()) return;

            if (this.m_clearBeforeLoad.Checked)
                this._decays.Clear();
            this._decays.AddRange(decays);

            this._paramsTable.Rows.Clear();
            this._chart.Series.Clear();
            var gradient = new ColorGradient(Program.GradientStart, Program.GradientEnd, this._decays.Count);
            foreach ((var i, var (name, decay)) in this._decays.Enumerate())
            {
                var color = gradient[i];

                var observed = new Series(name)
                {
                    ChartType = SeriesChartType.FastPoint,
                    Color = color,
                };
                var fitted = new Series($"{name} (fitted)")
                {
                    ChartType = SeriesChartType.FastLine,
                    BorderWidth = Program.FittedWidth,
                    BorderDashStyle = this.fitted_style,
                    Color = color,
                };

                var parameters = decay.EstimateParams();
                var f = parameters.GetFunction();
                foreach (var (time, signal) in decay)
                {
                    observed.Points.AddXY(time, signal);
                    fitted.Points.AddXY(time, f(time));
                }
                this._chart.Series.Add(observed);
                this._chart.Series.Add(fitted);

                var row = this._paramsTable.Add(name, decay, parameters, observed, fitted);
                row.Color = color;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        this.m_savePlot.Enabled = this.m_copy.Enabled = this.m_paste.Enabled = true;
        foreach (var row in this._paramsTable.ParamsRows)
        {
            row.ObservedSeries.Enabled = this.m_showObserved.Checked;
            row.FittedSeries.Enabled = this.m_showFitted.Checked;
        }
    } // private void LoadSources ()

    private void OpenSources(object? sender, EventArgs e)
        => OpenSources();

    private void OpenSources()
    {
        if (!BrowseSources()) return;
        LoadSources();
    } // private void OpenSources ()

    private void SelectColorGradient(object? sender, EventArgs e)
    {
        var picker = new ColorGradientPicker(Program.GradientStart, Program.GradientEnd)
        {
            StartPosition = FormStartPosition.CenterParent,
        };
        (Program.GradientStart, Program.GradientEnd) = picker.ShowDialog();

        SetColor();
    } // private void SelectColorGradient (object?, EventArgs)

    private void SetColor(object? sender, EventArgs e)
        => SetColor();

    private void SetColor()
    {
        if (this._decays.Count == 0) return;

        var gradient = new ColorGradient(Program.GradientStart, Program.GradientEnd, this._decays.Count);
        foreach ((var i, var row) in this._paramsTable.ParamsRows.Enumerate())
        {
            var color = gradient[i];
            row.Color = color;
            row.ObservedSeries.Color = color;
            row.FittedSeries.Color = color;
        }
    } // private void SetColor ()

    private void SelectAxisLabelFont(object? sender, EventArgs e)
    {
        using var fd = new FontDialog()
        {
            Font = Program.AxisLabelFont,
        };
        if (fd.ShowDialog() != DialogResult.OK) return;
        this.axisX.LabelStyle.Font = this.axisY.LabelStyle.Font = Program.AxisLabelFont = fd.Font;
    } // private void SelectAxisLabelFont (object?, EventArgs)

    private void SelectAxisTitleFont(object? sender, EventArgs e)
    {
        using var fd = new FontDialog()
        {
            Font = Program.AxisTitleFont,
        };
        if (fd.ShowDialog() != DialogResult.OK) return;
        this.axisX.TitleFont = this.axisY.TitleFont = Program.AxisTitleFont = fd.Font;
    } // private void SelectAxisTitleFont (object?, EventArgs)

    private void SevePlot(object? sender, EventArgs e)
    {
        if (this._decays.Count == 0) return;

        using var sfd = new SaveFileDialog()
        {
            Title = "Save plot",
            Filter = "PNG files|*.png|All files|*.*",
            DefaultExt = "png",
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;
        this._chart.SaveImage(sfd.FileName, ChartImageFormat.Png);
    } // private void SevePlot (object?, EventArgs)

    private void ToggleOberserved(object? sender, EventArgs e)
    {
        this.m_showObserved.Checked = !this.m_showObserved.Checked;
        var showObserved = this.m_showObserved.Checked;
        if (!showObserved && !this.m_showFitted.Checked)
        {
            this.m_showObserved.Checked = true;
            return;
        }

        foreach (var row in this._paramsTable.ParamsRows)
        {
            var s = row.ObservedSeries;
            if (s is null) continue;
            s.Enabled = showObserved;
        }
    } // private void ToggleOberserved (object?, EventArgs)

    private void ToggleFitted(object? sender, EventArgs e)
    {
        this.m_showFitted.Checked = !this.m_showFitted.Checked;
        var showFitted = this.m_showFitted.Checked;
        if (!showFitted && !this.m_showObserved.Checked)
        {
            this.m_showFitted.Checked = true;
            return;
        }

        foreach (var row in this._paramsTable.ParamsRows)
        {
            var s = row.FittedSeries;
            if (s is null) continue;
            s.Enabled = showFitted;
        }
    } // private void ToggleFitted (object?, EventArgs)

    private void CopyToClipboard(object? sender, EventArgs e)
    {
        if (this._decays.Count == 0) return;

        var header = "Name\tA0\tA\tα\tAT\tτt";

        var csv_rows = 
            this._paramsTable.ParamsRows
                .Where(row => row.Show)
                .Select(row => $"{row.Name}\t{row.A0:f2}\t{row.A:f2}\t{row.Alpha:f2}\t{row.AT:f2}\t{row.TauT:f2}");
        var text = header + Environment.NewLine + string.Join(Environment.NewLine, csv_rows);
        
        var data = new DataObject();
        data.SetData(DataFormats.UnicodeText, text);

        var csv_bytes = Encoding.UTF8.GetBytes(text);
        using var csv_stream = new MemoryStream(csv_bytes);
        data.SetData(DataFormats.CommaSeparatedValue, csv_stream);

        var table_rows =
            this._paramsTable.ParamsRows
                .Where(row => row.Show)
                .Select((row, i) => $"<tr id=\"row-{i+1}\">" +
                    $"<td class=\"cell-name\">{WebUtility.HtmlEncode(row.Name)}</td>" +
                    $"<td class=\"cell-a0\">{row.A0}</td><td class=\"cell-a\">{row.A}</td>" +
                    $"<td class=\"cell-alpha\">{row.Alpha}</td>" +
                    $"<td class=\"cell-at\">{row.AT}</td>" +
                    $"<td class=\"cell-taut\">{row.TauT}</td></tr>"
                );
        
        //lang=html
        var html = $$"""
            <html>
            <head>
                <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
                <style>
                    table { border-collapse:collapse; font-family:Arial; }
                    tr th, tr td { vertical-align:middle; text-align:right; padding-right:25px; }
                    #row-header th { border-top:1.0pt solid black; border-bottom:1.0pt solid black; }
                    #row-{{table_rows.Count()}} td { border-bottom:1.0pt solid black; }
                </style>
            </head>
            <body>
            <!--StartFragment-->
            <table>
            <tr id="row-header">
                <th class="cell-name">Name</th>
                <th class="cell-a0"><i>A</i><sub>0</sub> / &Delta;&micro;OD</th>
                <th class="cell-a">a / &micro;s<sup>&minus;1</sup></th>
                <th class="cell-alpha"><i>&alpha;</i></th>
                <th class="cell-at"><i>A</i><sub>T</sub> / &Delta;&micro;OD</th>
                <th class="cell-taut"><i>&tau;</i><sub>T</sub> / &micro;s</th>
            </tr>
            {{string.Join(string.Empty, table_rows)}}
            </table>
            <!--EndFragment-->
            </body>
            </html>
            """;

        var startHtml = 97;
        var endHtml = startHtml + html.Length;
        var startFragment = startHtml + html.IndexOf("<!--StartFragment-->");
        var endFragment = startHtml + html.IndexOf("<!--EndFragment-->");
        var metadata = $"""
            Version:1.0
            StartHTML:{startHtml:00000000}
            EndHTML:{endHtml:00000000}
            StartFragment:{startFragment:00000000}
            EndFragment:{endFragment:00000000}

            """;

        var html_bytes = Encoding.UTF8.GetBytes(metadata + html);
        using var html_stream = new MemoryStream(html_bytes);
        data.SetData(DataFormats.Html, html_stream);

        Clipboard.SetDataObject(data, true);
        FadingMessageBox.Show("Copied the table to clipboard", 0.8, 2000, 75, 0.05, this);
    } // private void CopyToClipboard (object?, EventArgs)

    private void PasteFromClipboard(object? sender, EventArgs e)
    {
        if (this._decays.Count == 0) return;
        if (!Clipboard.ContainsData(DataFormats.CommaSeparatedValue)) return;

        if (Clipboard.GetData(DataFormats.CommaSeparatedValue) is not MemoryStream stream) return;


        var bytes = stream.ToArray();
        var text = Encoding.UTF8.GetString(bytes);
        var rows = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Skip(1);
        this.cb_syncAlpha.Checked = this.cb_syncTauT.Checked = false;
        foreach (var row in rows)
        {
            var values = row.Split(',');
            var name = values[0];
            try
            {
                var tr = this._paramsTable[name];
                tr.A0 = double.Parse(values[1]);
                tr.A = double.Parse(values[2]);
                tr.Alpha = double.Parse(values[3]);
                tr.AT = double.Parse(values[4]);
                tr.TauT = double.Parse(values[5]);
            }
            catch { /* row not found */ }
        }
    } // private void PasteFromClipboard (object?, EventArgs)

    private static void EditFilenameFormat(object? sender, EventArgs e)
    {
        using var fnfd = new FileNameFormatDialog()
        {
            AMinusBFormat = Program.AMinusBSignalFormat,
            BFormat = Program.BSignalFormat,
        };
        if (fnfd.ShowDialog() != DialogResult.OK) return;

        Program.AMinusBSignalFormat = fnfd.AMinusBFormat;
        Program.BSignalFormat = fnfd.BFormat;
    } // private static void EditFilenameFormat (object?, EventArgs)
} // internal sealed partial class MainWindow : Form
