
// (c) 2024 Kazuki Kohzuki

using PowerT.Plugin;
using System.Runtime.InteropServices;

namespace PowerT.Data;

/// <summary>
/// Represents a smoother using the moving average method.
/// </summary>
[Guid("D2DCF761-0483-449E-B147-09E54FE1289A")]
internal class MovingAverageSmoother : ISmoother
{
    private int width = 30;

    /// <inheritdoc/>
    public string Name => "Moving Average";

    /// <inheritdoc/>
    public string Description => "Smoothes the data using the moving average method.";

    /// <inheritdoc/>
    public bool HasOption => true;

    /// <inheritdoc/>
    public void Initialize() { }

    /// <inheritdoc/>
    public void Dispose() { }

    /// <inheritdoc/>
    public IEnumerable<double> Smooth(IEnumerable<double> data)
    {
        var source = data.ToArray();
        var result = new double[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            var sum = 0.0;
            var count = 0;
            for (var j = i - this.width / 2; j <= i + this.width / 2; j++)
            {
                if (j < 0 || j >= source.Length) continue;
                sum += source[j];
                ++count;
            }
            result[i] = sum / count;
        }
        return result;
    } // public IEnumerable<double> Smooth (IEnumerable<double>)

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
} // internal class MovingAverageSmoother : ISmoother
