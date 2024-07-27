
// (c) 2024 Kazuki Kohzuki


namespace PowerT.Controls;

[DesignerCategory("Code")]
internal class BrowseButton<T> : Button where T : FileDialog, new()
{
    internal required PathBox Target { get; init; }

    internal bool AddExtension { get; set; } = true;

    internal bool AddToRecent { get; set; } = false;

    internal bool CheckFileExists { get; set; } = false;

    internal bool CheckPathExists { get; set; } = true;

    internal string DefaultExt { get; set; } = string.Empty;

    internal bool DereferenceLinks { get; set; } = true;

    internal string Filter { get; set; } = "All files|*.*";

    internal bool ShowHelp { get; set; } = false;

    internal bool SupportMultiDottedExtensions { get; set; } = false;

    internal string Title { get; set; } = "Select a file";

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
