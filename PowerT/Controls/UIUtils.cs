
// (c) 2024 Kazuki Kohzuki

using System.Text;
using System.Text.RegularExpressions;

namespace PowerT.Controls;

/// <summary>
/// Utility methods for UI.
/// </summary>
internal static partial class UIUtils
{
    [GeneratedRegex(@"(?<mantissa>.*)(E(?<exponent>.*))")]
    private static partial Regex re_expFormat();

    /// <summary>
    /// Formats the specified value in exponential notation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>String representation of the value in exponential notation.</returns>
    internal static string ExpFormatter(decimal value)
    {
        var s = value.ToString("E2");

        var match = re_expFormat().Match(s);
        if (!match.Success) return s;

        var mantissa = match.Groups["mantissa"].Value;
        var exponent = match.Groups["exponent"].Value;

        var sb = new StringBuilder(mantissa);
        sb.Append("×10");
        if (exponent.StartsWith('-'))
            sb.Append('⁻');  // U+207B

        var e = exponent[1..].TrimStart('0');
        if (e.Length == 0)
        {
            sb.Append('⁰');  // U+2070
        }
        else
        {
            // append unicode superscript
            foreach (var c in e)
            {
                /*
                 * Superscript of 1, 2, 3 are located in U+00Bx,
                 * whereas the rest in U+207x.
                 */
                sb.Append(c switch
                {
                    '1' => '¹',  // U+00B9
                    '2' => '²',  // U+00B2
                    '3' => '³',  // U+00B3
                    _ => (char)(c + 0x2040)  // U+2070 - U+2079
                });
            }
        }

        return sb.ToString();
    } // internal static string ExpFormatter (decimal)

    /// <summary>
    /// Calculates the inverted color of the specified color.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <returns>The inverted color.</returns>
    internal static Color CalcInvertColor(Color color)
    {
        var r = color.R;
        var g = color.G;
        var b = color.B;
        var m = 255;
        return Color.FromArgb(color.A, m - r, m - g, m - b);
    } // internal static Color CalcInvertColor (Color)
} // internal static partial class UIUtils
