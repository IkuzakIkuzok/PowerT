
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Controls.Concatenator;

[DesignerCategory("Code")]
internal sealed class SignalSelectButton : BrowseButton<OpenFileDialog>
{
    internal SignalSelectButton()
    {
        this.Text = "...";
        this.Filter = "CSV files|*.csv|All files|*.csv";
    } // ctor ()
} // internal sealed class SignalSelectButton : BrowseButton<OpenFileDialog>
