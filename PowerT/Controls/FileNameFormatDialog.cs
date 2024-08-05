
// (c) 2024 Kazuki Kohzuki

using PowerT.Controls.Concatenator;
using PowerT.Properties;

namespace PowerT.Controls;

[DesignerCategory("Code")]
internal sealed class FileNameFormatDialog : Form
{
    private readonly TextBox tb_ab, tb_b, tb_basename;
    private readonly Label lb_ab, lb_b;
    private readonly Button ok;

    internal string AMinusBFormat
    {
        get => this.tb_ab.Text;
        set => this.tb_ab.Text = value;
    }

    internal string BFormat
    {
        get => this.tb_b.Text;
        set => this.tb_b.Text = value;
    }

    internal FileNameFormatDialog()
    {
        this.Text = "Filename format";
        this.Size = this.MinimumSize = this.MaximumSize = new Size(400, 300);
        this.MaximizeBox = false;
        this.Icon = Resources.Icon;

        _ = new Label()
        {
            Text = "a\u2212b",
            Location = new Point(10, 10),
            Width = 40,
            Parent = this,
        };

        this.tb_ab = new TextBox()
        {
            Text = Program.AMinusBSignalFormat,
            Location = new Point(60, 10),
            Width = 300,
            PlaceholderText = "<BASENAME>-a-b-tdm.csv",
            Parent = this,
        };
        this.tb_ab.TextChanged += UpdateText;

        _ = new Label()
        {
            Text = "b",
            Location = new Point(10, 40),
            Width = 40,
            Parent = this,
        };

        this.tb_b = new TextBox()
        {
            Text = Program.BSignalFormat,
            Location = new Point(60, 40),
            Width = 300,
            PlaceholderText = "<BASENAME>-b.csv",
            Parent = this,
        };
        this.tb_b.TextChanged += UpdateText;

        _ = new Label()
        {
            Text = "Test basename",
            Location = new Point(10, 100),
            Width = 80,
            Parent = this,
        };

        this.tb_basename = new TextBox()
        {
            Text = "100us",
            Location = new Point(100, 100),
            Width = 260,
            Parent = this,
        };
        this.tb_basename.TextChanged += UpdateText;

        _ = new Label()
        {
            Text = "a\u2212b test",
            Location = new Point(10, 130),
            Width = 80,
            Parent = this,
        };

        this.lb_ab = new Label()
        {
            Location = new Point(100, 130),
            Width = 260,
            Parent = this,
        };

        _ = new Label()
        {
            Text = "b test",
            Location = new Point(10, 160),
            Width = 80,
            Parent = this,
        };

        this.lb_b = new Label()
        {
            Location = new Point(100, 160),
            Width = 260,
            Parent = this,
        };

        this.ok = new Button()
        {
            Text = "OK",
            Location = new Point(10, 200),
            Size = new Size(80, 30),
            DialogResult = DialogResult.OK,
            Parent = this,
        };

        _ = new Button()
        {
            Text = "Cancel",
            Location = new Point(100, 200),
            Size = new Size(80, 30),
            DialogResult = DialogResult.Cancel,
            Parent = this,
        };

        UpdateText();
    } // ctor ()

    private void UpdateText(object? sender, EventArgs e)
        => UpdateText();

    private void UpdateText()
    {
        var format_ab = this.tb_ab.Text;
        var format_b = this.tb_b.Text;
        if (string.IsNullOrEmpty(format_ab) || string.IsNullOrEmpty(format_b))
        {
            this.ok.Enabled = false;
            return;
        }
        else
        {
            this.ok.Enabled = true;
        }

        var basename = this.tb_basename.Text;
        if (string.IsNullOrEmpty(basename)) return;

        this.lb_ab.Text = FileNameHandler.GetFileName(basename, format_ab);
        this.lb_b.Text = FileNameHandler.GetFileName(basename, format_b);
    } // UpdateText ()
} // internal sealed class FileNameFormatDialog : Form
