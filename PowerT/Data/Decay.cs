
// (c) 2024 Kazuki Kohzuki

using System.Collections;

namespace PowerT.Data;

/// <summary>
/// Represents a decay data.
/// </summary>
internal sealed class Decay : IEnumerable<(double Time, double Signal)>
{
    private readonly double[] times, signals;

    /// <summary>
    /// An empty decay data.
    /// </summary>
    internal static readonly Decay Empty = new([], []);

    /// <summary>
    /// Gets the times.
    /// </summary>
    internal IEnumerable<double> Times => this.times;

    /// <summary>
    /// Gets the signals.
    /// </summary>
    internal IEnumerable<double> Signals => this.signals;

    /// <summary>
    /// Gets the minimum time.
    /// </summary>
    internal double TimeMin => this.times.Min();

    /// <summary>
    /// Gets the maximum time.
    /// </summary>
    internal double TimeMax => this.times.Max();

    /// <summary>
    /// Gets the time step.
    /// </summary>
    internal double TimeStep => this.times[1] - this.times[0];

    /// <summary>
    /// Gets the minimum signal.
    /// </summary>
    internal double SignalMin => this.signals.Min();

    /// <summary>
    /// Gets the maximum signal.
    /// </summary>
    internal double SignalMax => this.signals.Max();

    /// <summary>
    /// Gets the absolute decay data.
    /// </summary>
    internal Decay Absolute => new(this.times, this.signals.Select(Math.Abs).ToArray());

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
    /// <param name="timeScaling">The time scaling.</param>
    /// <param name="signalScaling">The signal scaling.</param>
    /// <returns>The decay data.</returns>
    internal static Decay FromFile(string filename, double timeScaling = 1.0, double signalScaling = 1.0)
    {
        try
        {
            var lines = File.ReadAllLines(filename);
            var times = new double[lines.Length];
            var signals = new double[lines.Length];
            for (var i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                times[i] = double.Parse(parts[0]) * timeScaling;
                signals[i] = double.Parse(parts[1]) * signalScaling;
            }
            return new(times, signals);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to read the file:\n{filename}", ex);
        }
    } // internal static Decay FromFile (string, [int], [int])

    /// <inheritdoc/>
    public IEnumerator<(double Time, double Signal)> GetEnumerator()
    {
        for (var i = 0; i < this.times.Length; i++)
            yield return (Time: this.times[i], Signal: this.signals[i]);
    } // public IEnumerator<(double Time, double Signal)> GetEnumerator()

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets the decay data of the specified range.
    /// </summary>
    /// <param name="start">The start time of the range.</param>
    /// <param name="end">The end time of the range.</param>
    /// <returns>The decay data of the specified range.</returns>
    internal Decay OfRange(double start, double end)
    {
        var startIndex = Array.BinarySearch(this.times, start);
        if (startIndex < 0) startIndex = ~startIndex;  // If not found, Array.BinarySearch returns the bitwise complement of the index of the next element.
        var endIndex = Array.BinarySearch(this.times, end);
        if (endIndex < 0) endIndex = ~endIndex;
        return new(this.times[startIndex..endIndex], this.signals[startIndex..endIndex]);
    } // internal Decay OfRange (double, double)

    /// <summary>
    /// Adds the time.
    /// </summary>
    /// <param name="time">The time</param>
    /// <returns>The decay data with the shifted time.</returns>
    internal Decay AddTime(double time)
        => new(this.times.Select(t => t + time).ToArray(), this.signals);

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

    #region operators

    public static Decay operator *(Decay decay, double scaling)
        => new(decay.times, decay.signals.Select(s => s * scaling).ToArray());

    public static Decay operator /(Decay decay, double scaling)
        => new(decay.times, decay.signals.Select(s => s / scaling).ToArray());

    #endregion operators
} // internal sealed class Decay : IEnumerable<(double Time, double Signal)>
