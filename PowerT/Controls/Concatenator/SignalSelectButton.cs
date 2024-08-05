
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Controls.Concatenator;

/// <summary>
/// Represents a button to browsw a signal file.
/// </summary>
[DesignerCategory("Code")]
internal sealed class SignalSelectButton : BrowseButton<OpenFileDialog>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignalSelectButton"/> class.
    /// </summary>
    internal SignalSelectButton()
    {
        this.Text = "...";
        this.Filter = "CSV files|*.csv|All files|*.csv";
    } // ctor ()
} // internal sealed class SignalSelectButton : BrowseButton<OpenFileDialog>
