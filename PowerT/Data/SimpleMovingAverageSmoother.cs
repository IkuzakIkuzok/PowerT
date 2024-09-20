
// (c) 2024 Kazuki Kohzuki

using System.Runtime.InteropServices;

namespace PowerT.Data;

/// <summary>
/// Represents a smoother using the simple moving average method.
/// </summary>
[Guid("D2DCF761-0483-449E-B147-09E54FE1289A")]
internal sealed class SimpleMovingAverageSmoother : MovingAverageSmootherBase
{
    /// <inheritdoc/>
    override public string Name => "Simple Moving Average";

    /// <inheritdoc/>
    override public string Description => "Smoothes the data using the moving average method.";

    /// <inheritdoc/>
    override public IEnumerable<double> Smooth(IEnumerable<double> data)
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
    } // override public IEnumerable<double> Smooth (IEnumerable<double>)
} // internal sealed class SimpleMovingAverageSmoother : MovingAverageSmootherBase
