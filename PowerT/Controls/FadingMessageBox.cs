
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Controls;

using System.Diagnostics;
using Timer = System.Windows.Forms.Timer;

[DesignerCategory("Code")]
internal class FadingMessageBox : Form
{
    private static FadingMessageBox? showing = null;

    private readonly Label label;
    private Timer? timer;
    private bool flag = false;
    private readonly Form parent;

    private int initialInterval;
    private int fadingInterval;
    private double fadeRate;

    override protected CreateParams CreateParams
    {
        get
        {
            const int WS_EX_TRANSPARENT = 0x20;
            var parms = base.CreateParams;
            parms.ExStyle |= WS_EX_TRANSPARENT;
            return parms;
        }
    }

    private FadingMessageBox(Form parent)
    {
        this.Size = this.MinimumSize = this.MaximumSize = new Size(500, 150);
        this.TopMost = true;
        this.ShowInTaskbar = false;
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.Manual;
        this.Top = parent.Top + (parent.Height - this.Height) / 2;
        this.Left = parent.Left + (parent.Width - this.Width) / 2;
        this.parent = parent;
        parent.FormClosed += OnParentClosed;

        this.label = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Gray,
            ForeColor = Color.White,
            Font = new Font(this.Font.Name, 22),
            TextAlign = ContentAlignment.MiddleCenter,
            Parent = this,
        };
    } // ctor (Form)

    internal static void Show(
        string text,
        double initialOpacity = 0.8,
        int initInterval = 2000,
        int fadeInterval = 75,
        double fadeRate = 0.05,
        Form? parentControl = null

    )
    {
        var form = new FadingMessageBox(parentControl ?? (FromHandle(Process.GetCurrentProcess().MainWindowHandle) as Form)!)
        {
            Opacity = initialOpacity,
        };
        form.label.Text = text;
        form.initialInterval = initInterval;
        form.fadingInterval = fadeInterval;
        form.fadeRate = fadeRate;

        showing?.Close();

        form.Show();
    } // internal static void Show (string, [double], [int], [int], [double], [Form])

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        this.parent.FormClosed -= OnParentClosed;
        this.timer?.Dispose();

        showing = null;
    } // protected override void OnClosed (EventArgs)

    override protected void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        showing = this;

        this.timer = new Timer()
        {
            Interval = this.initialInterval,
            Enabled = true,
        };
        this.timer.Tick += OnTimerTick;
    } // override protected void OnLoad (EventArgs)

    virtual protected void OnTimerTick(object? sender, EventArgs e)
    {
        if (!this.flag && this.timer is not null)
        {
            this.timer.Interval = this.fadingInterval;
            this.flag = true;
        }
        else
        {
            if (this.Opacity >= this.fadeRate)
                this.Opacity -= this.fadeRate;
            else
                Close();
        }
    } // virtual protected void OnTimerTick (object?, EventArgs)

    private void OnParentClosed(object? sender, EventArgs e)
        => Close();
} // internal class FadingMessageBox : Form
