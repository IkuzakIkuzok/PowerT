
// (c) 2024 Kazuki Kohzuki

namespace PowerT;

/// <summary>
/// Provides mathematical utilities.
/// </summary>
internal static class MathUtils
{
    /// <summary>
    /// Calculates the absolute value of the specified integer
    /// without throwing an exception in case of overflow.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The absolute avalue of the <paramref name="value"/>.</returns>
    /// <remarks>If the <paramref name="value"/> is <see cref="int.MinValue"/>,
    /// the return value is <see cref="int.MaxValue"/>, which is 1 less than the exact value.</remarks>
    internal static int SafeAbs(this int value)
    {
        try
        {
            return Math.Abs(value);
        }
        catch (OverflowException)
        {
            return int.MaxValue;
        }
    } // internal static int SafeAbs (this int)
} // internal static class MathUtils
