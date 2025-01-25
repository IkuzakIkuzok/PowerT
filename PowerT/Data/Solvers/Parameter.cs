
// (c) 2024 Kazuki KOHZUKI

namespace PowerT.Data.Solvers;

internal readonly struct Parameter
{
    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    required public string Name { get; init; }

    /// <summary>
    /// Gets the constraints of the parameter.
    /// </summary>
    public ParameterConstraints Constraints { get; init; } = ParameterConstraints.None;

    /// <summary>
    /// Gets the initial value of the parameter.
    /// </summary>
    public double InitialValue { get; init; } = 0.0;

    /// <summary>
    /// Gets a value indicating whether the parameter represents a magnitude.
    /// </summary>
    /// <remarks>
    /// The parameter will be inverted automatically with inverted mode if this property is set to <see langword="true"/>.
    /// </remarks>
    public bool IsMagnitude { get; init; } = false;

    public Parameter() { }
} // internal readonly struct Parameter
