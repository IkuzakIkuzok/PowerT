
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Data;

/// <summary>
/// Represents the decay parameters.
/// </summary>
internal sealed class Parameters
{
    /// <summary>
    /// Initial charge carrier absorption.
    /// </summary>
    internal double A0 { get; }

    /// <summary>
    /// An empirical parameter.
    /// </summary>
    internal double A { get; }

    /// <summary>
    /// Decay rate in the late region.
    /// </summary>
    internal double Alpha { get; }

    /// <summary>
    /// Initial triplet exciton absorption.
    /// </summary>
    internal double AT { get; }

    /// <summary>
    /// Lifetime of the triplet exciton.
    /// </summary>
    internal double TauT { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Parameters"/> class.
    /// </summary>
    /// <param name="a0">The A0 value.</param>
    /// <param name="a">The A value.</param>
    /// <param name="alpha">The alpha value.</param>
    /// <param name="at">The AT value.</param>
    /// <param name="tauT">The TauT value.</param>
    internal Parameters(double a0, double a, double alpha, double at, double tauT)
    {
        this.A0 = a0;
        this.A = a;
        this.Alpha = alpha;
        this.AT = at;
        this.TauT = tauT;
    } // ctor (double, double, double, double, double)

    /// <summary>
    /// Gets the function.
    /// </summary>
    /// <returns>The function.</returns>
    internal Func<double, double> GetFunction() =>
        x => this.A0 / Math.Pow(1 + this.A * x, this.Alpha) + this.AT * Math.Exp(-x / this.TauT);

    override public string ToString()
        => $"{this.A0} / ((1 + {this.A} * x) ^ {this.Alpha}) + {this.AT} * exp(-x / {this.TauT})";
} // internal sealed class Parameters
