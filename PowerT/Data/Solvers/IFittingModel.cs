
// (c) 2024 Kazuki KOHZUKI

namespace PowerT.Data.Solvers;

/// <summary>
/// Represents a fitting model.
/// </summary>
internal interface IFittingModel
{
    /// <summary>
    /// Gets the name of the model.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the model.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the parameter list of the model.
    /// </summary>
    public IReadOnlyList<Parameter> Parameters { get; }

    /// <summary>
    /// Gets a function based on the current model with the specified parameters.
    /// </summary>
    /// <param name="parameters">The parameters of the model.</param>
    /// <returns>A function based on the current model with the specified <paramref name="parameters"/>.</returns>
    public Func<double, double> GetFunction(IReadOnlyList<double> parameters);

    /// <summary>
    /// Gets the derivative functions of the model with respect to the parameters.
    /// </summary>
    /// <param name="parameters">The parameters of the model.</param>
    /// <returns>The derivative functions of the model with respect to the parameters.</returns>
    public Func<double, double[]> GetDerivatives(IReadOnlyList<double> parameters);
} // internal interface IFittingModel
