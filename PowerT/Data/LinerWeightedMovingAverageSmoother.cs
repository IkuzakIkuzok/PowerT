
// (c) 2024 Kazuki Kohzuki

using System.Runtime.InteropServices;

namespace PowerT.Data;

/// <summary>
/// Represents a smoother using the weighted moving average method.
/// </summary>
[Guid("E152F675-5600-46AE-82FC-83B1A76114A9")]
internal sealed class LinerWeightedMovingAverageSmoother : MovingAverageSmootherBase
{
    /// <inheritdoc/>
    override public string Name
        => "Liner Weighted Moving Average";

    /// <inheritdoc/>
    override public string Description
        => "Smoothes the data using the weighted moving average method.";

    /// <inheritdoc/>
    override public IEnumerable<double> Smooth(IEnumerable<double> data)
    {
        var source = data.ToArray();
        var result = new double[source.Length];
        var halfWidth = this.width >> 1;
        for (var i = 0; i < source.Length; i++)
        {
            var sum = 0.0;
            var count = 0.0;
            for (var j = i - halfWidth; j <= i + halfWidth; j++)
            {
                if (j < 0 || j >= source.Length) continue;
                var weight = 1.0 - MathUtils.SafeAbs(i - j) / halfWidth;
                sum += source[j] * weight;
                count += weight;
            }
            result[i] = sum / count;
        }
        return result;
    } // override public IEnumerable<double> Smooth (IEnumerable<double>)
} // internal sealed class LinerWeightedMovingAverageSmoother : MovingAverageSmootherBase
