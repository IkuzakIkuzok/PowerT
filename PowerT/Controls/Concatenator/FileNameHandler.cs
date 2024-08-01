
// (c) 2024 Kazuki Kohzuki

using System.Text.RegularExpressions;

namespace PowerT.Controls.Concatenator;

/// <summary>
/// Handles the filename of data files.
/// </summary>
internal static partial class FileNameHandler
{
    private static readonly Regex re_basename= BasenamePattern();

    /// <summary>
    /// Gets the filename from the basename and the format.
    /// </summary>
    /// <param name="basename">The basename.</param>
    /// <param name="format">The format.</param>
    /// <returns>The filename from the basename and the format.</returns>
    internal static string GetFileName(string basename, string format)
    {
        var filename = format;
        try
        {
            var ms = re_basename.Matches(format);
            foreach (Match m in ms)
            {
                var pattern = m.Value;
                var s_basename = basename;

                if (pattern != "<BASENAME>") // <BASENAME|...>
                {
                    var replaces = pattern[10..^1].Split('|');
                    foreach (var replace in replaces)
                    {
                        var kv = replace.Split('=');
                        s_basename = s_basename.Replace(kv[0], kv[1]);
                    }
                }

                filename = filename.Replace(pattern, s_basename);
            }

            return filename;
        }
        catch
        {
            return basename;
        }
    } // internal static string GetFileName (string, string)

    [GeneratedRegex(@"<BASENAME(\|[^|=]+=[^|=]+)*>", RegexOptions.Compiled)]
    private static partial Regex BasenamePattern();
} // internal static class FileNameHandler
