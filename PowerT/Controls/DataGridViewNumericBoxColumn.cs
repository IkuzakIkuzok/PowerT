
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Controls;

/// <summary>
/// Represents a column that contains <see cref="DataGridViewNumericBoxCell"/> objects.
/// </summary>
internal class DataGridViewNumericBoxColumn : DataGridViewColumn
{
    /// <summary>
    /// Gets or sets the bias of the digit order for incrementing.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the maximum value of the cells in the column.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the minimum value of the cells in the column.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridViewNumericBoxColumn"/> class.
    /// </summary>
    internal DataGridViewNumericBoxColumn() : this(0.0) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridViewNumericBoxColumn"/> class
    /// with the specified default value.
    /// </summary>
    /// <param name="defaultValue">The default value.</param>
    internal DataGridViewNumericBoxColumn(double defaultValue)
    {
        this.CellTemplate = new DataGridViewNumericBoxCell(defaultValue);
        this.SortMode = DataGridViewColumnSortMode.Automatic;
    } // ctor (double)
} // internal class DataGridViewNumericBoxColumn : DataGridViewColumn
