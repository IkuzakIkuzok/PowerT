
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Controls;

internal class DataGridViewNumericBoxCell : DataGridViewTextBoxCell
{
    internal double IncrementOrderBias { get; set; } = -1;

    internal double Maximum { get; set; } = double.MaxValue;
    internal double Minimum { get; set; } = double.MinValue;

    protected double defaultValue = 0.0;

    internal double DefaultValue
        => ((DataGridViewNumericBoxCell)this.OwningColumn.CellTemplate).defaultValue;

    public DataGridViewNumericBoxCell()
    {
        this.Style.Format = "N2";
    } // ctor ()

    public DataGridViewNumericBoxCell(double defaultValue) : this()
    {
        this.defaultValue = defaultValue;
    } // ctor (double)

    override public Type ValueType => typeof(double);

    override public object DefaultNewRowValue => this.DefaultValue;

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

    override protected void OnKeyDown(KeyEventArgs e, int rowIndex)
    {
        if (e.Alt && this.DataGridView is not null)
        {
            var additionalBias = e.Shift ? 10 : 1;

            if (e.KeyCode == Keys.Up)
            {
                var value = (double)GetValue(rowIndex);
                var increment = CalcIncrement() * additionalBias;
                SetValue(rowIndex, Math.Floor(value / increment) * increment + increment);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                var value = (double)GetValue(rowIndex);
                var decrement = CalcDecrement() * additionalBias;
                SetValue(rowIndex, Math.Ceiling(value / decrement) * decrement - decrement);
                e.Handled = true;
            }
        }

        if (!e.Handled)
            base.OnKeyDown(e, rowIndex);
    } // override protected void OnKeyDown (KeyEventArgs, int)

    protected virtual double GetLogValue()
    {
        var val = (double)GetValue(this.RowIndex);
        if (val <= 0)
            val = this.DefaultValue;
        return Math.Log10(val);
    } // protected virtual double GetLogValue ()

    protected virtual double CalcIncrement()
    {
        var order = Math.Floor(GetLogValue()) + this.IncrementOrderBias;
        return Math.Pow(10, order);
    } // protected virtual double CalcIncrement ()

    protected virtual double CalcDecrement()
    {
        var log = GetLogValue();
        var order = Math.Floor(log) + this.IncrementOrderBias;
        return log % 1 == 0 ? Math.Pow(10, order - 1) : Math.Pow(10, order);
    } // protected virtual double CalcDecrement ()
} // internal class DataGridViewNumericBoxCell : DataGridViewTextBoxCell
