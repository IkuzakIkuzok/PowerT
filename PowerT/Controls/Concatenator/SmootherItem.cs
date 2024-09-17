
// (c) 2024 Kazuki Kohzuki

using PowerT.Plugin;

namespace PowerT.Controls.Concatenator;

internal record SmootherItem(ISmoother? Smoother)
{
    public override string ToString()
        => this.Smoother?.Name ?? "None";
} // internal record SmootherItem (ISmoother?)
