
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Controls;

/// <summary>
/// Displays an editable numeric value in a <see cref="DataGridView"/> control.
/// </summary>
internal class DataGridViewNumericBoxCell : DataGridViewTextBoxCell
{
    /// <summary>
    /// Gets or sets the bias of the digit order for incrementing.
    /// </summary>
    internal double IncrementOrderBias { get; set; } = -1;

    /// <summary>
    /// Gets or sets the maximum value of the cell.
    /// </summary>
    internal double Maximum { get; set; } = double.MaxValue;

    /// <summary>
    /// Gets or sets the minimum value of the cell.
    /// </summary>
    internal double Minimum { get; set; } = double.MinValue;

    internal int Digit { get; set; } = 6;

    protected double defaultValue = 0.0;

    /// <summary>
    /// Gets the default value of the cell.
    /// </summary>
    internal double DefaultValue
        => ((DataGridViewNumericBoxCell)this.OwningColumn.CellTemplate).defaultValue;

    /// <summary>
    /// Initializes a new instance of the DataGridViewNumericBoxCell class.
    /// </summary>
    public DataGridViewNumericBoxCell()
    {
        this.Style.Format = "N2";
    } // ctor ()

    /// <summary>
    /// Initializes a new instance of the DataGridViewNumericBoxCell class
    /// with the specified default value.
    /// </summary>
    /// <param name="defaultValue">The default value of the cell.</param>
    public DataGridViewNumericBoxCell(double defaultValue) : this()
    {
        this.defaultValue = defaultValue;
    } // ctor (double)

    override public Type ValueType => typeof(double);

    override public object DefaultNewRowValue => this.DefaultValue;

    /// <inheritdoc/>
    override protected object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
    {
        if (value is double d)
            return d.ToString("N2");
        return base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
    } // override protected object GetFormattedValue (object, int, ref DataGridViewCellStyle, TypeConverter, TypeConverter, DataGridViewDataErrorContexts)

    override protected bool SetValue(int rowIndex, object value)
    {
        if (value is string s && double.TryParse(s, out var d))
        {
            d = Math.Max(this.Minimum, Math.Min(this.Maximum, d));
            return base.SetValue(rowIndex, d);
        }
        return base.SetValue(rowIndex, value);
    } // override protected bool SetValue (int, object)

    protected double GetDoubleValue(int rowIndex)
        => Math.Round((double)GetValue(rowIndex), this.Digit);

    override protected void OnKeyDown(KeyEventArgs e, int rowIndex)
    {
        if (e.Alt && this.DataGridView is not null)
        {
            var additionalBias = e.Shift ? 10 : 1;

            if (e.KeyCode == Keys.Up)
            {
                var value = GetDoubleValue(rowIndex);
                var increment = CalcIncrement() * additionalBias;
                if (increment == 0) increment = 1;
                SetValue(rowIndex, Math.Floor(Math.Round(value / increment, this.Digit)) * increment + increment);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                var value = GetDoubleValue(rowIndex);
                var decrement = CalcDecrement() * additionalBias;
                if (decrement == 0) decrement = 1;
                var newValue = Math.Ceiling(Math.Round(value / decrement, this.Digit)) * decrement - decrement;
                SetValue(rowIndex, newValue);
                e.Handled = true;
            }
        }

        if (!e.Handled)
            base.OnKeyDown(e, rowIndex);
    } // override protected void OnKeyDown (KeyEventArgs, int)

    /// <summary>
    /// Gets the logarithmic value of the cell.
    /// </summary>
    /// <returns>The logarithmic value.</returns>
    protected virtual double GetLogValue()
    {
        var val = (double)GetValue(this.RowIndex);
        if (val <= 0)
            val = this.DefaultValue;
        return Math.Log10(val);
    } // protected virtual double GetLogValue ()

    /// <summary>
    /// Calculates the increment value.
    /// </summary>
    /// <returns>The increment value.</returns>
    protected virtual double CalcIncrement()
    {
        var order = Math.Floor(GetLogValue()) + this.IncrementOrderBias;
        return Math.Pow(10, order);
    } // protected virtual double CalcIncrement ()

    /// <summary>
    /// Calculates the decrement value.
    /// </summary>
    /// <returns>The decrement value.</returns>
    protected virtual double CalcDecrement()
    {
        var log = GetLogValue();
        var order = Math.Floor(log) + this.IncrementOrderBias;
        return log % 1 == 0 ? Math.Pow(10, order - 1) : Math.Pow(10, order);
    } // protected virtual double CalcDecrement ()
} // internal class DataGridViewNumericBoxCell : DataGridViewTextBoxCell
