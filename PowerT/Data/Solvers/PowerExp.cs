
// (c) 2024 Kazuki KOHZUKI

namespace PowerT.Data.Solvers;

internal sealed class PowerExp : IFittingModel
{
    private static readonly Parameter[] parameters = [
        new() { Name = "A0"   , InitialValue = 1e3, IsMagnitude = true },
        new() { Name = "a"    , InitialValue = 1.0, Constraints = ParameterConstraints.Positive },
        new() { Name = "Alpha", InitialValue = 0.4, Constraints = ParameterConstraints.Positive },
        new() { Name = "AT"   , InitialValue = 1e3, IsMagnitude = true },
        new() { Name = "τT"   , InitialValue = 5.0, Constraints = ParameterConstraints.Positive },
    ];

    internal static IReadOnlyList<double> InitialValues
        => parameters.Select(p => p.InitialValue).ToArray();

    /// <inheritdoc/>
    public string Name => "Power-law + Exp";

    /// <inheritdoc/>
    public string Description => "Power-law + exponential model";

    /// <inheritdoc/>
    public IReadOnlyList<Parameter> Parameters => parameters;

    /// <inheritdoc/>
    public Func<double, double> GetFunction(IReadOnlyList<double> parameters)
    {
        var a0 = parameters[0];
        var a = parameters[1];
        var alpha = parameters[2];
        var at = parameters[3];
        var tauT = parameters[4];
        return x => a0 / Math.Pow(1 + a * x, alpha) + at * Math.Exp(-x / tauT);
    } // public Func<double, double> GetFunction (IReadOnlyList<double> parameters)

    /// <inheritdoc/>
    public Func<double, double[]> GetDerivatives(IReadOnlyList<double> parameters)
    {
        var a0 = parameters[0];
        var a = parameters[1];
        var alpha = parameters[2];
        var at = parameters[3];
        var tauT = parameters[4];

        return (x) =>
        {
            var ax = a * x;
            var pow = Math.Pow(1 + ax, -alpha);
            var exp = Math.Exp(-x / tauT);

            var d_a0 = 1 / pow;
            var d_a = -a0 * x * Math.Pow(1 + ax, -1 - alpha) * alpha;
            var d_alpha = -a0 * Math.Log(1 + ax) * pow;
            var d_at = exp;
            var d_tauT = at * x * exp / (tauT * tauT);
            return [d_a0, d_a, d_alpha, d_at, d_tauT];
        };
    } // public Func<double, double[]> GetDerivatives (IReadOnlyList<double>)
} // internal sealed class PowerExp : IFittingModel
