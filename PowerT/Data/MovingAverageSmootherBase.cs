
// (c) 2024 Kazuki Kohzuki

using PowerT.Plugin;

namespace PowerT.Data;

/// <summary>
/// Represents a base class for smoothers using the moving average method.
/// </summary>
internal abstract class MovingAverageSmootherBase : ISmoother
{
    protected int width = 30;

    /// <inheritdoc/>
    abstract public string Name { get; }

    /// <inheritdoc/>
    abstract public string Description { get; }

    /// <inheritdoc/>
    public bool HasOption => true;

    /// <inheritdoc/>
    public void Initialize() { }

    /// <inheritdoc/>
    public void Dispose() { }

    /// <inheritdoc/>
    abstract public IEnumerable<double> Smooth(IEnumerable<double> data);

    /// <inheritdoc/>
    public bool SetOption(string option)
    {
        if (int.TryParse(option, out var width) && width > 0)
        {
            this.width = width;
            return true;
        }
        return false;
    } // public bool SetOption (string)

    /// <inheritdoc/>
    public string GetOption() => this.width.ToString();
} // internal abstract class MovingAverageSmootherBase : ISmoother
