
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Controls.Text;

internal static class PaintHandlerBuilder
{
    internal static PaintEventHandler Create(params Chunk[] chunks)
        => (sender, e) =>
        {
            var x_offset = 0;
            if (sender is not Control control) return;
            foreach (var chunk in chunks)
            {
                var size = chunk.Size <= 0 ? control.Font.Size : chunk.Size;
                x_offset += chunk.XOffset;
                var y_offset = chunk.YOffset;
                e.Graphics.DrawString(chunk.Text, new Font(control.Font.FontFamily, size), Brushes.Black, x_offset, y_offset);
                x_offset += (int)e.Graphics.MeasureString(chunk.Text, new Font(control.Font.FontFamily, size)).Width;
            }
        };

    private static readonly DataGridViewPaintParts PaintPartsNoText = DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground;

    internal static DataGridViewCellPaintingEventHandler CreateCellHandler(int row, int col, params Chunk[] chunks)
        => (sender, e) =>
        {
            if (e.RowIndex != row || e.ColumnIndex != col) return;

            var x_offset = e.CellBounds.X;
            if (sender is not DataGridView dgv) return;
            if (e.Graphics is not Graphics g) return;

            e.Paint(e.ClipBounds, PaintPartsNoText);

            foreach (var chunk in chunks)
            {
                var size = chunk.Size <= 0 ? dgv.Font.Size : chunk.Size;
                x_offset += chunk.XOffset;
                var y_offset = e.CellBounds.Y + chunk.YOffset;
                using var font = new Font(dgv.Font.FontFamily, size);
                g.DrawString(chunk.Text, font, Brushes.Black, x_offset, y_offset);
                x_offset += (int)g.MeasureString(chunk.Text, font).Width;
            }
            e.Handled = true;
        };
} // internal static class PaintHandlerBuilder
