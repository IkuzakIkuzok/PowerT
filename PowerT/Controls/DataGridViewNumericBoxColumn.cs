
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Controls;

internal class DataGridViewNumericBoxColumn : DataGridViewColumn
{
    internal double IncrementOrderBias
    {
        get => ((DataGridViewNumericBoxCell)this.CellTemplate).IncrementOrderBias;
        set
        {
            ((DataGridViewNumericBoxCell)this.CellTemplate).IncrementOrderBias = value;
            var dgw = this.DataGridView;
            if (dgw is not null)
            {
                for (var i = 0; i < dgw.RowCount; i++)
                {
                    var cell = (DataGridViewNumericBoxCell)dgw[this.Index, i];
                    cell.IncrementOrderBias = value;
                }
            }
        }
    }

    internal double Maximum
    {
        get => ((DataGridViewNumericBoxCell)this.CellTemplate).Maximum;
        set
        {
            ((DataGridViewNumericBoxCell)this.CellTemplate).Maximum = value;
            var dgw = this.DataGridView;
            if (dgw is not null)
            {
                for (var i = 0; i < dgw.RowCount; i++)
                {
                    var cell = (DataGridViewNumericBoxCell)dgw[this.Index, i];
                    cell.Maximum = value;
                }
            }
        }
    }

    internal double Minimum
    {
        get => ((DataGridViewNumericBoxCell)this.CellTemplate).Minimum;
        set
        {
            ((DataGridViewNumericBoxCell)this.CellTemplate).Minimum = value;
            var dgw = this.DataGridView;
            if (dgw is not null)
            {
                for (var i = 0; i < dgw.RowCount; i++)
                {
                    var cell = (DataGridViewNumericBoxCell)dgw[this.Index, i];
                    cell.Minimum = value;
                }
            }
        }
    }

    internal DataGridViewNumericBoxColumn() : this(0.0) { }

    internal DataGridViewNumericBoxColumn(double defaultValue)
    {
        this.CellTemplate = new DataGridViewNumericBoxCell(defaultValue);
    } // ctor (double)
} // internal class DataGridViewNumericBoxColumn : DataGridViewColumn
