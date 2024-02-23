
// (c) 2024 Kazuki Kohzuki

namespace PowerT.Controls;

/// <summary>
/// Represents a Windows spin box (also known as an up-down control) that displays numeric values in a logarithmic scale.
/// </summary>
[DesignerCategory("Code")]
internal class LogarithmicNumericUpDown : NumericUpDown
{
    protected decimal Decrement => CalcDecrement();

    override public void UpButton()
    {
        this.Value = Math.Min(checked(this.Value + this.Increment), this.Maximum);
    } // override public void UpButton ()

    override public void DownButton()
    {
        this.Value = Math.Max(checked(this.Value - this.Decrement), this.Minimum);
    } // override public void DownButton ()

    override protected void OnMouseWheel(MouseEventArgs e)
    {
        if (e is HandledMouseEventArgs hme) hme.Handled = true;

        if (e.Delta > 0) UpButton();
        else DownButton();
    } // override protected void OnMouseWheel (MouseEventArgs)

    override protected void OnValueChanged(EventArgs e)
    {
        this.Increment = CalcIncrement((double)this.Value);
        base.OnValueChanged(e);
    } // override protected void OnValueChanged (EventArgs)

    protected virtual decimal CalcIncrement(double value)
    {
        var log = Math.Log10(value);
        var order = Math.Floor(log);
        return (decimal)Math.Pow(10, order);
    } // protected virtual double CalcIncrement (double)

    protected virtual decimal CalcDecrement()
    {
        var log = Math.Log10((double)this.Value);
        return log % 1 == 0 ? this.Increment / 10 : this.Increment;
    } // protected virtual double CalcDecrement ()
} // internal class LogarithmicNumericUpDown : NumericUpDown
