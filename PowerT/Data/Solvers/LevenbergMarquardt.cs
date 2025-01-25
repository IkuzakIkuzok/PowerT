
// (c) 2024 Kazuki KOHZUKI

using Numbers = System.Collections.Generic.IReadOnlyList<double>;

namespace PowerT.Data.Solvers;

/*
 * Levenberg-Marquardt algorithm for nonlinear least squares fitting.
 * 
 * Problem:
 *  Minimize J = (1/2) ΣΣ F(𝕩, 𝕦)²
 *  where 𝕩 is the observed data, and 𝕦 is the model parameters.
 *  F = f(𝕦) - 𝕩 is difference between the observed data and the model function f,
 *  which gives F(𝕩, 𝕦) = 0 if the model is perfect.
 *  
 * Solution:
 *  The Levenberg-Marquardt algorithm is written as:
 *      𝕦ₖ₊₁ = 𝕦ₖ - (H + λD[H])⁻¹∇J(𝕦ₖ)
 *  where 𝕦ₖ is the current parameters, λ is the damping parameter, 
 *  H is the Hessian matrix, D[H] is the diagonal of the Hessian matrix,
 *  and ∇J is the gradient of the cost function.
 *  
 *  The damping parameter is set to a large value if the estimated parameters are far from the optimal solution.
 *  This results in a steepest descent method.
 *  In contrast, the damping parameter is set to a small value
 *  if the estimated parameters are close to the optimal solution, which leads to a Gauss-Newton method.
 *  
 *  The step size vector Δ𝕦ₖ is calculated by solving the linear equation (H + λD[H])Δ𝕦ₖ = -∇J.
 *  There is no need to calculate the inverse of the Hessian matrix.
 *  
 *  Calculation of the Hessian matrix, which requires the second derivative, is computationally expensive.
 *  Therefore, it is approximated by the first derivatives.
 *      ∂J/∂𝕦ᵢ = ΣΣ F(𝕩, 𝕦) ∂F(𝕩, 𝕦)/∂𝕦ᵢ
 *      ∂J²/∂𝕦ᵢ∂𝕦ⱼ = ΣΣ (∂F(𝕩, 𝕦)/∂𝕦ᵢ ∂F(𝕩, 𝕦)/∂𝕦ⱼ + F(𝕩, 𝕦) ∂²F(𝕩, 𝕦)/∂𝕦ᵢ∂𝕦ⱼ)
 *  If the 𝕦 is close to the optimal solution, the second term is negligible, row.e.,
 *      ∂J²/∂𝕦ᵢ∂𝕦ⱼ ≃ ΣΣ (∂F(𝕩, 𝕦)/∂𝕦ᵢ ∂F(𝕩, 𝕦)/∂𝕦ⱼ)
 *  
 *  The first derivatives of F is equal to the partial derivatives of the model function f,
 *  because the observed data 𝕩 is constant.
 *  The calculation of the partial derivatives is computationally expensive,
 *  and the same partial derivatives are used multiple times.
 *  Therefore, the partial derivatives are calculated once and stored in a cache.
 */

internal sealed class LevenbergMarquardt
{
    /// <summary>
    /// Gets the fitting model.
    /// </summary>
    internal IFittingModel Model { get; }

    /// <summary>
    /// Gets the x values.
    /// </summary>
    internal Numbers X => this.x;

    /// <summary>
    /// Gets the y values.
    /// </summary>
    internal Numbers Y => this.y;

    /// <summary>
    /// Gets the parameters.
    /// </summary>
    internal Numbers Parameters => this.parameters;

    /// <summary>
    /// Gets the lambda (damping parameter).
    /// </summary>
    internal double Lambda { get; private set; } = 0.001;

    /// <summary>
    /// Gets the maximum iteration count.
    /// </summary>
    internal int MaxIteration { get; init; } = 100;

    /// <summary>
    /// Gets the minimum delta chi-squared.
    /// </summary>
    internal double MinimumDeltaChi2 { get; init; } = 1e-30;

    /// <summary>
    /// Gets the derivative threshold.
    /// </summary>
    internal double DerivativeThreshold { get; init; } = 1e-4;

    private readonly Numbers x, y;
    private readonly double[] parameters;
    private readonly double[] incrementedParameters;
    private readonly ParameterConstraints[] constraints;

    private readonly int numberOfParameters, numberOfDataPoints;
    private readonly double[,] hessian;  // Hessian matrix with the damping parameter on the diagonal
    private readonly double[] gradient;
    private readonly double[][] derivatives;  // Cache for the partial derivatives
    private Func<double, double> func = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="LevenbergMarquardt"/> class.
    /// </summary>
    /// <param name="model">The fitting model.</param>
    /// <param name="x">The x values.</param>
    /// <param name="y">The y values.</param>
    /// <exception cref="ArgumentException">The number of <paramref name="x"/> and <paramref name="y"/> values must be the same.</exception>
    internal LevenbergMarquardt(IFittingModel model, Numbers x, Numbers y) : this(model, x, y, model.Parameters.Select(p => p.InitialValue).ToArray()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LevenbergMarquardt"/> class
    /// with the specified initial parameters.
    /// </summary>
    /// <param name="model">The fitting model.</param>
    /// <param name="x">The x values.</param>
    /// <param name="y">The y values.</param>
    /// <param name="parameters">The initial parameters.</param>
    /// <exception cref="ArgumentException">The number of <paramref name="x"/> and <paramref name="y"/> values must be the same.</exception>
    internal LevenbergMarquardt(IFittingModel model, Numbers x, Numbers y, Numbers parameters)
    {
        if (x.Count != y.Count)
            throw new ArgumentException("The number of x and y values must be the same.");

        this.Model = model;
        this.x = x;
        this.y = y;

        this.numberOfParameters = parameters.Count;
        this.numberOfDataPoints = x.Count;

        this.parameters = new double[this.numberOfParameters];
        Array.Copy(parameters.ToArray(), 0, this.parameters, 0, this.numberOfParameters);
        this.constraints = model.Parameters.Select(p => p.Constraints).ToArray();

        this.incrementedParameters = new double[this.numberOfParameters];
        this.hessian = new double[this.numberOfParameters, this.numberOfParameters];
        this.gradient = new double[this.numberOfParameters];
        this.derivatives = new double[this.numberOfDataPoints][];
        for (var i = 0; i < this.numberOfDataPoints; ++i)
            this.derivatives[i] = new double[this.numberOfParameters];
    } // ctor (IFittingModel, Numbers, Numbers, Numbers)

    /// <summary>
    /// Fits the model to the data.
    /// </summary>
    internal void Fit()
    {
        var iterCount = 0;

        double chi2, incrementedChi2;
        do
        {
            this.func = this.Model.GetFunction(this.parameters);
            chi2 = CalcChi2();
            ComputeDerivatives();
            CalcHessian();
            CalcGradient();

            SolveIncrements();

            incrementedChi2 = CalcIncrementedChi2();

            CheckConstraints();
            if (double.IsNaN(incrementedChi2)) break;
            if (incrementedChi2 >= chi2)
            {
                this.Lambda *= 10;
            }
            else
            {
                this.Lambda /= 10;
                UpdateParameters();
            }

            ++iterCount;
        } while (!CheckStop(iterCount, chi2, incrementedChi2));
    } // internal void Fit ()

    private double CalcChi2(Numbers parameters)
    {
        var func = this.Model.GetFunction(parameters);
        return this.x.Zip(this.y, (xi, yi) => yi - func(xi)).Select(diff => diff * diff).Sum();
    } // private double CalcChi2 (Numbers)

    private double CalcChi2()
        => CalcChi2(this.parameters);

    private double CalcIncrementedChi2()
        => CalcChi2(this.incrementedParameters);

    private void SolveIncrements()
    {
        /*
         * We need Δ=α⁻¹β for the increments.
         * However, calculating the inverse of a matrix is computationally expensive.
         * Therefore, we solve the linear equation αΔ=β instead.
         * `hessian` and `gradient` are overwritten, but they are not needed anymore within the iteration.
         */

        // Gaussian elimination
        for (var row = 0; row < this.numberOfParameters; ++row)
        {
            var pivot = this.hessian[row, row];
            if (pivot == 0)
            {
                this.gradient[row] = 0;
            }
            else
            {
                for (var otherRow = row + 1; otherRow < this.numberOfParameters; ++otherRow)
                {
                    var ratio = this.hessian[otherRow, row] / pivot;
                    for (var col = 0; col < this.numberOfParameters; ++col)
                        this.hessian[otherRow, col] -= ratio * this.hessian[row, col];
                    this.gradient[otherRow] -= ratio * this.gradient[row];
                }
                for (var col = 0; col < this.numberOfParameters; ++col)
                    this.hessian[row, col] /= pivot;
                this.gradient[row] /= pivot;
            }
        }

        for (var i = this.numberOfParameters - 1; i > 0; --i)
        {
            var b = this.gradient[i];
            for (var j = i - 1; j >= 0; --j)
                this.gradient[j] -= this.hessian[j, i] * b;
        }

        for (var i = 0; i < this.numberOfParameters; ++i)
            this.incrementedParameters[i] = this.parameters[i] + this.gradient[i];
    } // private void SolveIncrements ()

    private void CalcHessian()
    {
        for (var row = 0; row < this.numberOfParameters; ++row)
            for (var col = 0; col < this.numberOfParameters; ++col)
            {
                var h = 0.0;
                for (var i = 0; i < this.numberOfDataPoints; ++i)
                    h += this.derivatives[i][row] * this.derivatives[i][col];
                if (row == col) h *= 1 + this.Lambda;
                this.hessian[row, col] = h;
            }
    } // private void CalcHessian ()

    private void CalcGradient()
    {
        for (var row = 0; row < this.numberOfParameters; ++row)
        {
            var g = 0.0;
            for (var i = 0; i < this.numberOfDataPoints; ++i)
                g += (this.y[i] - this.func(this.x[i])) * this.derivatives[i][row];
            this.gradient[row] = g;
        }
    } // private void CalcGradient ()

    private void ComputeDerivatives()
    {
        var d = this.Model.GetDerivatives(this.parameters);
        for (var i = 0; i < this.numberOfDataPoints; ++i)
            Array.Copy(
                d(this.x[i]),
                0,
                this.derivatives[i],
                0,
                this.numberOfParameters
            );
    } // private void ComputeDerivativesCacheAnalytically ()

    private void UpdateParameters()
        => Array.Copy(this.incrementedParameters, 0, this.parameters, 0, this.numberOfParameters);

    private void CheckConstraints()
    {
        for (var i = 0; i < this.numberOfParameters; ++i)
        {
            if (this.constraints[i] == ParameterConstraints.Positive && this.parameters[i] <= 0)
                this.parameters[i] = 1e-10;
            if (this.constraints[i] == ParameterConstraints.NonNegative && this.parameters[i] < 0)
                this.parameters[i] = 0;
            if (this.constraints[i] == ParameterConstraints.Integer)
                this.parameters[i] = Math.Round(this.parameters[i]);
        }
    } // private void CheckConstraints ()

    private bool CheckStop(int iterCount, double chi2, double incrementedChi2)
    {
        if (iterCount > this.MaxIteration) return true;
        return Math.Abs(chi2 - incrementedChi2) < this.MinimumDeltaChi2;
    } // private bool CheckStop (int, double, double)
} // internal sealed class LevenbergMarquardt
