
// (c) 2024 Kazuki Kohzuki


namespace PowerT.Controls;

/// <summary>
/// Represents a button to browse a file.
/// </summary>
/// <typeparam name="T">The type of the file dialog.</typeparam>
[DesignerCategory("Code")]
internal class BrowseButton<T> : Button where T : FileDialog, new()
{
    /// <summary>
    /// Gets or sets the target <see cref="PathBox"/>.
    /// </summary>
    internal required PathBox Target { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the dialog box automatically adds an extension to a file name if the user omits the extension.
    /// </summary>
    internal bool AddExtension { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the dialog box adds the file being opened or saved to the recent list.
    /// </summary>
    internal bool AddToRecent { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the dialog box displays a warning if the user specifies a file name that does not exist.
    /// </summary>
    internal bool CheckFileExists { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the dialog box displays a warning if the user specifies a path that does not exist.
    /// </summary>
    internal bool CheckPathExists { get; set; } = true;

    /// <summary>
    /// Gets or sets the default file name extension.
    /// </summary>
    internal string DefaultExt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the dialog box returns the location of the file referenced by the shortcut or whether it returns the location of the shortcut (.lnk).
    /// </summary>
    internal bool DereferenceLinks { get; set; } = true;

    /// <summary>
    /// Gets or sets the current file name filter string, which determines the choices that appear in the "Save as file type" or "Files of type" box in the dialog box.
    /// </summary>
    internal string Filter { get; set; } = "All files|*.*";

    /// <summary>
    /// Gets or sets a value indicating whether the Help button is displayed in the file dialog box.
    /// </summary>
    internal bool ShowHelp { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the dialog box supports displaying and saving files that have multiple file name extensions.
    /// </summary>
    internal bool SupportMultiDottedExtensions { get; set; } = false;

    /// <summary>
    /// Gets or sets the file dialog box title.
    /// </summary>
    internal string Title { get; set; } = "Select a file";

    /// <summary>
    /// Gets or sets a value indicating whether the dialog box accepts only valid Win32 file names.
    /// </summary>
    internal bool ValidateNames { get; set; } = true;

    override protected void OnClick(EventArgs e)
    {
        base.OnClick(e);

        using var dialog = new T()
        {
            AddExtension = this.AddExtension,
            AddToRecent = this.AddToRecent,
            CheckFileExists = this.CheckFileExists,
            CheckPathExists = this.CheckPathExists,
            DefaultExt = this.DefaultExt,
            DereferenceLinks = this.DereferenceLinks,
            Filter = this.Filter,
            ShowHelp = this.ShowHelp,
            SupportMultiDottedExtensions = this.SupportMultiDottedExtensions,
            Title = this.Title,
            ValidateNames = this.ValidateNames,
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        this.Target.Text = dialog.FileName;
    } // override protected void OnClick (EventArgs)
} // internal class BrowseButton<T> : Button where T : FileDialog, new()
