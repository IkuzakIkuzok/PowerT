﻿
// (c) 2024 Kazuki Kohzuki

using PowerT.Data;
using System.Net;
using System.Text;

namespace PowerT.Controls;

internal static class ClipboardHandler
{
    #region copy to

    private const string CSV_HEADER = "Name\tA0\tA\tα\tAT\tτt";

    /// <summary>
    /// Copies the rows to the clipboard.
    /// </summary>
    /// <param name="rows">The rows.</param>
    /// <param name="toText">If set to <c>true</c>, copy as plain text.</param>
    /// <param name="toCsv">If set to <c>true</c>, copy as CSV.</param>
    /// <param name="toHtml">If set to <c>true</c>, copy as HTML table.</param>
    /// <exception cref="ArgumentException">At least one of the formats must be selected.</exception>
    /// <exception cref="System.Runtime.InteropServices.ExternalException">An error occurred when accessing the Clipboard.
    /// The exception details will include an <c>HResult</c> that identifies the specific error;
    /// see <see cref="System.Runtime.InteropServices.ErrorWrapper.ErrorCode"/>.</exception>
    /// <exception cref="OutOfMemoryException">The clipboard content is too large.</exception>
    /// <exception cref="OverflowException">The <paramref name="rows"/> contains too many rows.</exception>
    /// <exception cref="ThreadStateException">The current thread is not in single-threaded apartment (STA) mode.</exception>
    internal static void CopyToClipboard(IEnumerable<ParamsRow> rows, bool toText = true, bool toCsv = true, bool toHtml = true)
    {
        if (!(toText || toCsv || toHtml))
        {
            throw new ArgumentException("At least one of the formats must be selected.");
        }

        var data = new DataObject();

        var text = CreateCsvContent(rows);
        if (toText)
        {
            data.SetData(DataFormats.Text, text);
        }

        if (toCsv)
        {
            using var csv = new MemoryStream(text.ToBytes());
            data.SetData(DataFormats.CommaSeparatedValue, csv);
        }

        if (toHtml)
        {
            using var html = new MemoryStream(CreateHtmlContent(rows).ToBytes());
            data.SetData(DataFormats.Html, html);
        }
        
        Clipboard.SetDataObject(data, true);
    } // internal static void CopyToClipboard (IEnumerable<ParamsRow>)

    /// <summary>
    /// Creates the CSV content.
    /// </summary>
    /// <param name="rows">The rows.</param>
    /// <returns>The CSV content.</returns>
    /// <exception cref="OutOfMemoryException">The CSV content is too large.</exception>
    private static string CreateCsvContent(IEnumerable<ParamsRow> rows)
    {
        var csv_rows = rows.Select(row => $"{row.Name}\t{row.A0:f2}\t{row.A:f2}\t{row.Alpha:f2}\t{row.AT:f2}\t{row.TauT:f2}");
        return CSV_HEADER + Environment.NewLine + string.Join(Environment.NewLine, csv_rows);
    } // private static string CreateCsvContent (IEnumerable<ParamsRow>)

    /// <summary>
    /// Creates the HTML content.
    /// </summary>
    /// <param name="rows">The rows.</param>
    /// <returns>The HTML content.</returns>
    /// <exception cref="OverflowException">The <paramref name="rows"/> contains too many rows.</exception>
    /// <exception cref="OutOfMemoryException">The HTML content is too large.</exception>
    private static string CreateHtmlContent(IEnumerable<ParamsRow> rows)
    {
        var table_rows = rows.Select((row, i)
            => $"<tr id=\"row-{i + 1}\">" +
                $"<td class=\"cell-name\">{WebUtility.HtmlEncode(row.Name)}</td>" +
                $"<td class=\"cell-a0\">{row.A0}</td><td class=\"cell-a\">{row.A}</td>" +
                $"<td class=\"cell-alpha\">{row.Alpha}</td>" +
                $"<td class=\"cell-at\">{row.AT}</td>" +
                $"<td class=\"cell-taut\">{row.TauT}</td></tr>"
            );

        //lang=html
        var html = $$"""
            <html>
            <head>
                <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
                <style>
                    table { border-collapse:collapse; font-family:Arial; }
                    tr th, tr td { vertical-align:middle; text-align:right; padding-right:25px; }
                    #row-header th { border-top:1.0pt solid black; border-bottom:1.0pt solid black; }
                    #row-{{table_rows.Count()}} td { border-bottom:1.0pt solid black; }
                </style>
            </head>
            <body>
            <!--StartFragment-->
            <table>
            <tr id="row-header">
                <th class="cell-name">Name</th>
                <th class="cell-a0"><i>A</i><sub>0</sub> / &Delta;&micro;OD</th>
                <th class="cell-a">a / &micro;s<sup>&minus;1</sup></th>
                <th class="cell-alpha"><i>&alpha;</i></th>
                <th class="cell-at"><i>A</i><sub>T</sub> / &Delta;&micro;OD</th>
                <th class="cell-taut"><i>&tau;</i><sub>T</sub> / &micro;s</th>
            </tr>
            {{string.Join(string.Empty, table_rows)}}
            </table>
            <!--EndFragment-->
            </body>
            </html>
            """;

        var startHtml = 97;
        var endHtml = startHtml + html.Length;
        var startFragment = startHtml + html.IndexOf("<!--StartFragment-->");
        var endFragment = startHtml + html.IndexOf("<!--EndFragment-->");
        var metadata = $"""
            Version:1.0
            StartHTML:{startHtml:00000000}
            EndHTML:{endHtml:00000000}
            StartFragment:{startFragment:00000000}
            EndFragment:{endFragment:00000000}

            """;
        
        return metadata + html;
    } // private static string CreateHtmlContent (IEnumerable<ParamsRow>)

    #endregion copy to

    #region paste from

    /// <summary>
    /// Gets the rows from the clipboard.
    /// </summary>
    /// <returns>The names and parameters of the rows.</returns>
    /// <exception cref="System.Runtime.InteropServices.ExternalException">An error occurred when accessing the Clipboard.
    /// The exception details will include an <c>HResult</c> that identifies the specific error;
    /// see <see cref="System.Runtime.InteropServices.ErrorWrapper.ErrorCode"/>.</exception>
    /// <exception cref="ThreadStateException">The current thread is not in single-threaded apartment (STA) mode.</exception>
    internal static IEnumerable<(string Name, Parameters Parameters)> GetRowsFromClipboard()
    {
#pragma warning disable Ex0105  // Exception handler does not work well in iterators.
        if (!Clipboard.ContainsData(DataFormats.CommaSeparatedValue)) yield break;
        if (Clipboard.GetData(DataFormats.CommaSeparatedValue) is not MemoryStream stream) yield break;
#pragma warning restore

        var csv = stream.ToArray().ToText();
        var rows =
            csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
               .Skip(1);  // Skip header line

        foreach (var row in rows)
        {
            var values = row.Split(',');
            if (values.Length < 6) continue;  // Skip invalid row (e.g. empty line
            
            var name = values[0];
            if (!double.TryParse(values[1], out var a0)) continue;
            if (!double.TryParse(values[2], out var a)) continue;
            if (!double.TryParse(values[3], out var alpha)) continue;
            if (!double.TryParse(values[4], out var at)) continue;
            if (!double.TryParse(values[5], out var taut)) continue;
            var parameters = new Parameters(a0, a, alpha, at, taut);
            yield return (name, parameters);
        }
    } // internal static IEnumerable<(string Name, Parameters Parameters)> GetRowsFromClipboard ()

    #endregion paste from

    /// <summary>
    /// Converts the specified text to a byte array.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>UTF-8 encoded byte array.</returns>
    private static byte[] ToBytes(this string text)
        => Encoding.UTF8.GetBytes(text);

    /// <summary>
    /// Converts the specified byte array to a text.
    /// </summary>
    /// <param name="bytes">The byte array.</param>
    /// <returns>UTF-8 decoded text.</returns>
    private static string ToText(this byte[] bytes)
        => Encoding.UTF8.GetString(bytes);
} // internal static class ClipboardHandler
