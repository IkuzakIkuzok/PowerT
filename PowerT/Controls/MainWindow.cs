
// (c) 2024 Kazuki Kohzuki

using PowerT.Controls.Concatenator;
using PowerT.Controls.Text;
using PowerT.Data;
using PowerT.Properties;
using System.Diagnostics;
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
    private readonly ToolStripMenuItem m_showObserved, m_showFit;
    private readonly ToolStripMenuItem m_savePlot, m_copy, m_paste, m_clearBeforeLoad;

    private readonly List<(string, Decay)> _decays = [];

    private ChartDashStyle fit_style = ChartDashStyle.Solid;

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

        AddDummySeries();

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
            AllowUserToDeleteRows = true,
            Parent = this._params_container.Panel1,
        };
        this._paramsTable.Sorted += SetColor;
        this._paramsTable.RowMoved += SetColor;
        this._paramsTable.UserDeletingRow += (sender, e) =>
        {
            if (e.Row is not ParamsRow row) return;
            var observed = row.ObservedSeries;
            var fit = row.FitSeries;
            if (observed is not null) this._chart.Series.Remove(observed);
            if (fit is not null) this._chart.Series.Remove(fit);
        };
        this._paramsTable.UserDeletedRow += SetColor;

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

        _ = new DisplayRangeSelector(this.axisX, this.axisY)
        {
            Location = new(10, 60),
            Parent = this._params_container.Panel2,
        };

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

        var m_fit = new ToolStripMenuItem()
        {
            Text = "&Fit Curves"
        };
        m_view.DropDownItems.Add(m_fit);

        #region fit.width

        var m_fit_width = new ToolStripMenuItem()
        {
            Text = "&Width"
        };
        m_fit.DropDownItems.Add(m_fit_width);

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
                Program.FitWidth = w;
                foreach (var row in this._paramsTable.ParamsRows)
                {
                    var s = row.FitSeries;
                    if (s is null) continue;
                    s.BorderWidth = w;
                }
            };
            m_fit_width.DropDownItems.Add(item);
        }
        m_fit_width.DropDownOpening += (sender, e) =>
        {
            foreach (ToolStripMenuItem item in m_fit_width.DropDownItems)
                item.Checked = (int)item.Tag! == Program.FitWidth;
        };

        #endregion fit.width

        #region fit.style

        var m_fit_style = new ToolStripMenuItem()
        {
            Text = "&Style"
        };
        m_fit.DropDownItems.Add(m_fit_style);

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
                this.fit_style = style;
                foreach (var row in this._paramsTable.ParamsRows)
                {
                    var s = row.FitSeries;
                    if (s is null) continue;
                    s.BorderDashStyle = style;
                }
            };
            m_fit_style.DropDownItems.Add(item);
        }
        m_fit_style.DropDownOpening += (sender, e) =>
        {
            foreach (ToolStripMenuItem item in m_fit_style.DropDownItems)
                item.Checked = (ChartDashStyle)item.Tag! == this.fit_style;
        };

        #endregion fit.style

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

        this.m_showFit = new()
        {
            Text = "&Fit",
            Checked = true,
            ShortcutKeys = Keys.Control | Keys.F,
        };
        this.m_showFit.Click += ToggleFit;
        m_data.DropDownItems.Add(this.m_showFit);

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

        var m_clearRows = new ToolStripMenuItem()
        {
            Text = "Clear &data",
        };
        m_clearRows.Click += ClearRows;
        m_data.DropDownItems.Add(m_clearRows);

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
        try
        {
            this.tb_sources.Text = string.Join(';', files);
        }
        catch (OutOfMemoryException)
        {
            if (files.Length > 0)
                this.tb_sources.Text = files[0];
        }
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
        try
        {
            this.tb_sources.Text = string.Join(';', ofd.FileNames);
        }
        catch (OutOfMemoryException)
        {
            this.tb_sources.Text = ofd.FileName;
        }
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
            AddDummySeries();
            var gradient = new ColorGradient(Program.GradientStart, Program.GradientEnd, this._decays.Count);
            foreach ((var i, var (name, decay)) in this._decays.Enumerate())
            {
                var color = gradient[i];

                var observed = new Series(name)
                {
                    ChartType = SeriesChartType.FastPoint,
                    Color = color,
                };
                var fit = new Series($"{name} (fit)")
                {
                    ChartType = SeriesChartType.FastLine,
                    BorderWidth = Program.FitWidth,
                    BorderDashStyle = this.fit_style,
                    Color = color,
                };

                var parameters = decay.EstimateParams();
                var f = parameters.GetFunction();
                foreach (var (time, signal) in decay)
                {
                    observed.Points.AddXY(time, signal);
                    fit.Points.AddXY(time, f(time));
                }
                this._chart.Series.Add(observed);
                this._chart.Series.Add(fit);

                var row = this._paramsTable.Add(name, decay, parameters, observed, fit);
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
            row.FitSeries.Enabled = this.m_showFit.Checked;
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
        this._paramsTable.ParamsRows.SetGradient(gradient);
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

    private void AddDummySeries()
    {
        var dummy = new Series()
        {
            ChartType = SeriesChartType.Point,
            IsVisibleInLegend = false,
            IsXValueIndexed = false,
        };
        dummy.Points.AddXY(1e-6, 1e-6);
        this._chart.Series.Add(dummy);
    } // private void AddDummySeries ()

    private void ClearRows(object? sender, EventArgs e)
    {
        if (this._paramsTable.Rows.Count == 0) return;
        var dr = MessageBox.Show(
            "Clear all rows?",
            "Clear",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );
        if (dr != DialogResult.Yes) return;
        ClearRows();
    } // private void ClearRows (object?, EventArgs)

    private void ClearRows()
    {
        while (this._paramsTable.Rows.Count > 0)
            RemoveRow(this._paramsTable[0]);
    } // private void ClearRows ()

    private void RemoveRow(ParamsRow row)
    {
        var observed = row.ObservedSeries;
        var fit = row.FitSeries;
        if (observed is not null) this._chart.Series.Remove(observed);
        if (fit is not null) this._chart.Series.Remove(fit);
        this._paramsTable.Rows.Remove(row);
        SetColor();
    } // private void RemoveRow (ParamsRow)

    private void ToggleOberserved(object? sender, EventArgs e)
    {
        this.m_showObserved.Checked = !this.m_showObserved.Checked;
        var showObserved = this.m_showObserved.Checked;
        if (!showObserved && !this.m_showFit.Checked)
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

    private void ToggleFit(object? sender, EventArgs e)
    {
        this.m_showFit.Checked = !this.m_showFit.Checked;
        var showFit = this.m_showFit.Checked;
        if (!showFit && !this.m_showObserved.Checked)
        {
            this.m_showFit.Checked = true;
            return;
        }

        foreach (var row in this._paramsTable.ParamsRows)
        {
            var s = row.FitSeries;
            if (s is null) continue;
            s.Enabled = showFit;
        }
    } // private void ToggleFit (object?, EventArgs)

    private void CopyToClipboard(object? sender, EventArgs e)
    {
        var rows =this._paramsTable.ParamsRows.Where(row => row.Show);
        if (!rows.Any()) return;

        try
        {
            ClipboardHandler.CopyToClipboard(rows);
            FadingMessageBox.Show("Copied the table to clipboard", 0.8, 2000, 75, 0.05, this);
        }
        catch
        {
            FadingMessageBox.Show("Failed to copy the table to clipboard", 0.8, 2000, 75, 0.05, this);
        }
    } // private void CopyToClipboard (object?, EventArgs)

    private void PasteFromClipboard(object? sender, EventArgs e)
    {
        if (this._decays.Count == 0) return;

        try
        {
            var rows = ClipboardHandler.GetRowsFromClipboard();
            if (!rows.Any()) return;

            this.cb_syncAlpha.Checked = this.cb_syncTauT.Checked = false;
            foreach (var (name, parameters) in rows)
            {
                try
                {
                    var tr = this._paramsTable[name];
                    tr.Parameters = parameters;
                }
                catch { /* row not found */ }
            }
        }
        catch
        {
            FadingMessageBox.Show("Failed to paste the table from clipboard", 0.8, 2000, 75, 0.05, this);
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
