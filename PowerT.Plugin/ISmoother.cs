
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Plugin;

/// <summary>
/// Represents a smoother.
/// </summary>
public interface ISmoother : IPlugin
{
    /// <summary>
    /// Gets a value indicating whether the smoother has an option.
    /// </summary>
    bool HasOption { get; }

    /// <summary>
    /// Smoothes the data.
    /// </summary>
    /// <param name="data">The data to be smoothed.</param>
    /// <returns>The smoothed data.</returns>
    public IEnumerable<double> Smooth(IEnumerable<double> data);

    /// <summary>
    /// Sets the option.
    /// </summary>
    /// <param name="option">The option.</param>
    /// <returns><see langword="true"/> if the option is set successfully; otherwise, <see langword="false"/>.</returns>
    public bool SetOption(string option);

    /// <summary>
    /// Gets the current option.
    /// </summary>
    /// <returns></returns>
    public string GetOption();
} // public interface ISmoother
