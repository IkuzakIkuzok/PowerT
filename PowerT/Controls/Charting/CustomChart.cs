
// (c) 2024 Kazuki Kohzuki

using System.Windows.Forms.DataVisualization.Charting;

namespace PowerT.Controls.Charting;

/// <summary>
/// Represents a customizable chart control.
/// </summary>
[DesignerCategory("Code")]
internal class CustomChart : Chart
{
    protected Point mouseLeftMoveStartPx, mouseRightMoveStartPx;

    protected bool isMouseLeftMoving, isMouseRightMoving;

    /// <summary>
    /// Gets the X-axis.
    /// </summary>
    protected Axis AxisX => this.ChartAreas[0].AxisX;

    /// <summary>
    /// Gets the Y-axis.
    /// </summary>
    protected Axis AxisY => this.ChartAreas[0].AxisY;

    /// <summary>
    /// Gets or sets the minimum value of the X-axis.
    /// </summary>
    internal double AxisXMinimum { get; set; } = double.MinValue;

    /// <summary>
    /// Gets or sets the maximum value of the X-axis.
    /// </summary>
    internal double AxisXMaximum { get; set; } = double.MaxValue;

    /// <summary>
    /// Gets or sets the minimum value of the Y-axis.
    /// </summary>
    internal double AxisYMinimum { get; set; } = double.MinValue;

    /// <summary>
    /// Gets or sets the maximum value of the Y-axis.
    /// </summary>
    internal double AxisYMaximum { get; set; } = double.MaxValue;

    /// <summary>
    /// Gets the rate of movement of the X-axis.
    /// </summary>
    protected double MoveRateX => CalcMoveRate(this.AxisX);

    /// <summary>
    /// Gets the rate of movement of the Y-axis.
    /// </summary>
    protected double MoveRateY => CalcMoveRate(this.AxisY);

    /// <summary>
    /// Gets or sets the bias of movement of the X-axis.
    /// </summary>
    internal double MoveXBias { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the bias of movement of the Y-axis.
    /// </summary>
    internal double MoveYBias { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the threshold of movement in pixels.
    /// </summary>
    internal int ThresholdPx { get; set; } = 5;

    /// <summary>
    /// Occurs when the range of the X-axis is changed.
    /// </summary>
    internal event AxisRangeChangedEventHandler? AxisXRangeChanged;

    /// <summary>
    /// Occurs when the range of the Y-axis is changed.
    /// </summary>
    internal event AxisRangeChangedEventHandler? AxisYRangeChanged;

    /// <inheritdoc/>
    override protected void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (!IsMouseInChartArea()) return;

        if (e.Button == MouseButtons.Left)
        {
            this.mouseLeftMoveStartPx = e.Location;
            this.isMouseLeftMoving = true;
        }
        if (e.Button == MouseButtons.Right)
        {
            this.mouseRightMoveStartPx = e.Location;
            this.isMouseRightMoving = true;
        }
    } // protected override void OnMouseDown (MouseEventArgs)

    /// <inheritdoc/>
    override protected void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var mousePx = GetCursorPositionPx();

        if (this.isMouseLeftMoving) // Move minimum
        {
            var moveXPx = mousePx.X - this.mouseLeftMoveStartPx.X;
            var moveYPx = mousePx.Y - this.mouseLeftMoveStartPx.Y;

            if (moveXPx.SafeAbs() > this.ThresholdPx)
            {
                if (this.AxisX.IsLogarithmic)
                {
                    var moveX = Math.Pow(10, -moveXPx * this.MoveRateX * this.MoveXBias);
                    if (this.AxisX.Minimum * moveX < this.AxisXMinimum || !double.IsFinite(moveX))
                        this.AxisX.Minimum = this.AxisXMinimum;
                    else
                        this.AxisX.Minimum *= moveX;
                }
                else
                {
                    var moveX = -moveXPx * this.MoveRateX;
                    moveX *= this.MoveXBias;
                    if (this.AxisX.Minimum + moveX < this.AxisXMinimum || !double.IsFinite(moveX))
                        this.AxisX.Minimum = this.AxisXMinimum;
                    else
                        this.AxisX.Minimum += moveX;
                }
                OnAxisXRangeChanged(new(this.AxisX));
                this.mouseLeftMoveStartPx.X = mousePx.X;
            }

            if (moveYPx.SafeAbs() > this.ThresholdPx)
            {
                if (this.AxisY.IsLogarithmic)
                {
                    var moveY = Math.Pow(10, moveYPx * this.MoveRateY * this.MoveYBias);
                    if (this.AxisY.Minimum * moveY < this.AxisYMinimum || !double.IsFinite(moveY))
                        this.AxisY.Minimum = this.AxisYMinimum;
                    else
                        this.AxisY.Minimum *= moveY;
                }
                else
                {
                    var moveY = moveYPx * this.MoveRateY;
                    moveY *= this.MoveYBias;
                    if (this.AxisY.Minimum + moveY < this.AxisYMinimum || !double.IsFinite(moveY))
                        this.AxisY.Minimum = this.AxisYMinimum;
                    else
                        this.AxisY.Minimum += moveY;
                }
                OnAxisYRangeChanged(new(this.AxisY));
                this.mouseLeftMoveStartPx.Y = mousePx.Y;
            }
        } // move minimum

        if (this.isMouseRightMoving) // move maximum
        {
            var moveXPx = mousePx.X - this.mouseRightMoveStartPx.X;
            var moveYPx = mousePx.Y - this.mouseRightMoveStartPx.Y;

            if (moveXPx.SafeAbs() > this.ThresholdPx)
            {
                if (this.AxisX.IsLogarithmic)
                {
                    var moveX = Math.Pow(10, -moveXPx * this.MoveRateX * this.MoveXBias);
                    if (this.AxisX.Maximum * moveX > this.AxisXMaximum || !double.IsFinite(moveX))
                        this.AxisX.Maximum = this.AxisXMaximum;
                    else
                        this.AxisX.Maximum *= moveX;
                }
                else
                {
                    var moveX = -moveXPx * this.MoveRateX;
                    moveX *= this.MoveXBias;
                    if (this.AxisX.Maximum + moveX > this.AxisXMaximum || !double.IsFinite(moveX))
                        this.AxisX.Maximum = this.AxisXMaximum;
                    else
                        this.AxisX.Maximum += moveX;
                }
                OnAxisXRangeChanged(new(this.AxisX));
                this.mouseRightMoveStartPx.X = mousePx.X;
            }

            if (moveYPx.SafeAbs() > this.ThresholdPx)
            {
                if (this.AxisY.IsLogarithmic)
                {
                    var moveY = Math.Pow(10, moveYPx * this.MoveRateY * this.MoveYBias);
                    if (this.AxisY.Maximum * moveY > this.AxisYMaximum || !double.IsFinite(moveY))
                        this.AxisY.Maximum = this.AxisYMaximum;
                    else
                        this.AxisY.Maximum *= moveY;
                }
                else
                {
                    var moveY = moveYPx * this.MoveRateY;
                    moveY *= this.MoveYBias;
                    if (this.AxisY.Maximum + moveY > this.AxisYMaximum || !double.IsFinite(moveY))
                        this.AxisY.Maximum = this.AxisYMaximum;
                    else
                        this.AxisY.Maximum += moveY;
                }
                OnAxisYRangeChanged(new(this.AxisY));
                this.mouseRightMoveStartPx.Y = mousePx.Y;
            }
        } // move maximum
    } // protected override void OnMouseMove (MouseEventArgs)

    /// <inheritdoc/>
    override protected void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (e.Button == MouseButtons.Left)
            this.isMouseLeftMoving = false;
        if (e.Button == MouseButtons.Right)
            this.isMouseRightMoving = false;
    } // protected override void OnMouseUp (MouseEventArgs)

    /// <summary>
    /// Gets the range of the specified axis.
    /// </summary>
    /// <param name="axis">The axis.</param>
    /// <returns>The minimum and maximum value of the axis.</returns>
    protected static (double Min, double Max) GetRange(Axis axis)
         => (axis.Minimum, axis.Maximum);

    /// <summary>
    /// Gets the range of the specified axis in pixels.
    /// </summary>
    /// <param name="axis">The axis.</param>
    /// <returns>The minimum and maximum pixels of the axis.</returns>
    protected static (double Min, double Max) GetRangePx(Axis axis)
    {
        var range = GetRange(axis);
        return GetRangePx(axis, range);
    } // protected static (double, double) GetRangePx (Axis)

    protected static (double Min, double Max) GetRangePx(Axis axis, (double Min, double Max) range)
        => (axis.ValueToPixelPosition(range.Min), axis.ValueToPixelPosition(range.Max));

    /// <summary>
    /// Checks whether the mouse cursor is in the chart area.
    /// </summary>
    /// <returns><c>true</c> if the mouse cursor is in the chart area; otherwise, <c>false</c>.</returns>
    protected bool IsMouseInChartArea()
    {
        var mousePx = GetCursorPositionPx();
        var (xMinPx, xMaxPx) = GetRangePx(this.AxisX);
        var (yMaxPx, yMinPx) = GetRangePx(this.AxisY); // The direction of vertical axis is reversed

        return xMinPx <= mousePx.X && mousePx.X <= xMaxPx
            && yMinPx <= mousePx.Y && mousePx.Y <= yMaxPx;
    } // private bool IsMouseInChartArea ()

    /// <summary>
    /// Gets the cursor position in pixels.
    /// </summary>
    /// <returns>The cursor position.</returns>
    protected Point GetCursorPositionPx()
        => PointToClient(System.Windows.Forms.Cursor.Position);

    /// <summary>
    /// Calculates the rate of movement of the specified axis.
    /// </summary>
    /// <param name="axis">The axis.</param>
    /// <returns>The movement scale per pixel.</returns>
    protected virtual double CalcMoveRate(Axis axis)
    {
        var range = GetRange(axis);
        var (Min, Max) = GetRangePx(axis, range);
        var sizePx = Math.Abs(Max - Min);

        if (axis.IsLogarithmic)
            return Math.Log10(range.Max / range.Min) / sizePx;
        else
            return (range.Max - range.Min) / sizePx;
    } // protected virtual double CalcMoveRate (Axis)

    /// <summary>
    /// Raises the <see cref="AxisXRangeChanged"/> event.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnAxisXRangeChanged(AxisRangeChangedEventArgs e)
        => AxisXRangeChanged?.Invoke(this, e);

    /// <summary>
    /// Raises the <see cref="AxisYRangeChanged"/> event.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnAxisYRangeChanged(AxisRangeChangedEventArgs e)
        => AxisYRangeChanged?.Invoke(this, e);
} // internal class CustomChart : Chart
