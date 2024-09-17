
// (c) 2024 Kazuki Kohzuki

using PowerT.Plugin;

namespace PowerT.Controls.Concatenator;

[DesignerCategory("Code")]
internal class SmootherSelector : ComboBox
{
    internal ISmoother? SelectedSmoother => (this.SelectedItem as SmootherItem)?.Smoother;

    internal SmootherSelector() : base()
    {
        this.DropDownStyle = ComboBoxStyle.DropDownList;

        this.Items.Add(new SmootherItem(null));

        foreach (var smoother in PluginManager.Smoothers)
            this.Items.Add(new SmootherItem(smoother));
    } // ctor ()
} // internal class SmootherSelector : ComboBox
