
// (c) 2024 Kazuki Kohzuki

using System.Collections;

namespace PowerT.Data;

/// <summary>
/// Represents a decay data.
/// </summary>
internal sealed class Decay : IEnumerable<(double, double)>
{
    private readonly double[] times, signals;

    internal IEnumerable<double> Times => this.times;

    /// <summary>
    /// Initializes a new instance of the <see cref="Decay"/> class.
    /// </summary>
    /// <param name="times">The times.</param>
    /// <param name="signals">The signals.</param>
    /// <exception cref="ArgumentException">times and signals must have the same length.</exception></exception>
    internal Decay(double[] times, double[] signals)
    {
        if (times.Length != signals.Length)
            throw new ArgumentException("times and signals must have the same length.");

        this.times = times;
        this.signals = signals;
    } // ctor (double[], double[])

    /// <summary>
    /// Reads a decay data from a file.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <returns>The decay data.</returns>
    internal static Decay FromFile(string filename)
    {
        try
        {
            var lines = File.ReadAllLines(filename);
            var times = new double[lines.Length];
            var signals = new double[lines.Length];
            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                times[i] = double.Parse(parts[0]);
                signals[i] = double.Parse(parts[1]);
            }
            return new(times, signals);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to read the file:\n{filename}", ex);
        }
    } // internal static Decay FromFile (string)

    public IEnumerator<(double, double)> GetEnumerator()
    {
        for (var i = 0; i < this.times.Length; i++)
            yield return (this.times[i], this.signals[i]);
    } // public IEnumerator<(double, double)> GetEnumerator()

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Estimates the parameters.
    /// </summary>
    /// <returns>The estimated parameters.</returns>
    internal Parameters EstimateParams()
    {
        // TODO: Implement more 'nice' estimation (´･_･`)

        var lastHalf = this.times.Length >> 1;
        var logX = this.times.Select(x => Math.Log(x)).TakeLast(lastHalf);
        var logY = this.signals.Select(y => Math.Log(y)).TakeLast(lastHalf);

        (var sp, var ip) = LinearRegression(logX, logY);

        var eip = double.IsNaN(ip) ? this.signals[0] : Math.Exp(ip);
        var a0 = Math.Round(eip / 100) * 100;
        var a = 1.0; // How to estimate?
        var alpha = -Math.Round(sp, 2);

        var at = Math.Max(Math.Round(this.signals.Max() / 100) * 100 - a0, 0);
        var tauT = 0.3;
        return new(a0, a, alpha, at, tauT);
    } // internal Parameters EstimateParams()

    private static (double slope, double intercept) LinearRegression(IEnumerable<double> x, IEnumerable<double> y)
    {
        var n = x.Count();
        var Sx = x.Sum();
        var Sy = y.Sum();
        var Sxx = x.Select(x => x * x).Sum();
        var Sxy = x.Zip(y).Select(p => p.First * p.Second).Sum();
        var denom = n * Sxx - Sx * Sx;
        var slope = (n * Sxy - Sx * Sy) / denom;
        var intercept = (Sxx * Sy - Sx * Sxy) / denom;
        return (slope, intercept);
    } // private static (double, double) LinearRegression (IEnumerable<double>, IEnumerable<double>)
} // internal sealed class Decay : IEnumerable<(double, double)>
