
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Controls;

/// <summary>
/// Represents a text box for file paths.
/// </summary>
[DesignerCategory("Code")]
internal class PathBox : TextBox
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PathBox"/> class.
    /// </summary>
    internal PathBox()
    {
        this.AllowDrop = true;
    } // ctor ()

    override protected void OnDragEnter(DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) ?? false)
        {
            e.Effect = DragDropEffects.Copy;
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    } // override protected void OnDragEnter (e)

    override protected void OnDragDrop(DragEventArgs e)
    {
        if (!(e.Data?.GetDataPresent(DataFormats.FileDrop) ?? false)) return;
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files) return;
        if (files.Length == 0) return;
        this.Text = files[0];
    } // override protected void OnDragDrop (DragEventArgs)
} // internal class PathBox : TextBox
