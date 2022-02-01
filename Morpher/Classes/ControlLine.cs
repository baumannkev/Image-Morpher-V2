using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Morpher
{
    public class ControlLine
    {
        public ControlLineCanvas ControlLineCanvas { get; private set; }
        public ControlLineStartThumb Start { get; private set; }
        public ControlLineMidThumb Mid { get; private set; }
        public ControlLineEndThumb End { get; private set; }
        public LineGeometry Line { get; private set; }
        public Path Path { get; private set; }
        public int Index
        {
            get { return ControlLineCanvas.ControlLines.IndexOf(this); }
            private set { Index = value; }
        }
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public ControlLine(ControlLineCanvas controlLineCanvas, double x, double y)
        {
            ControlLineCanvas = controlLineCanvas;

            Start = new ControlLineStartThumb(this, x, y);
            Mid = new ControlLineMidThumb(this, x, y);
            End = new ControlLineEndThumb(this, x, y);

            Line = new LineGeometry();
            Path = new Path();
            Path.Stroke = Brushes.MidnightBlue;
            Path.StrokeThickness = 1;
            Path.Data = Line;
            ControlLineCanvas.canvas.Children.Add(Path);
        }

        public ControlLine(ControlLineCanvas controlLineCanvas, ControlLine controlLine)
        {
            ControlLineCanvas = controlLineCanvas;

            Point startPos = controlLine.Start.GetPos();
            Point endPos = controlLine.End.GetPos();

            Start = new ControlLineStartThumb(this, startPos.X, startPos.Y);
            End = new ControlLineEndThumb(this, endPos.X, endPos.Y);
            Vector v = Point.Subtract(startPos, endPos);
            Mid = new ControlLineMidThumb(this, endPos.X + v.X / 2, endPos.Y + v.Y / 2);
            Canvas.SetZIndex(End, 2);

            Line = new LineGeometry();
            Path = new Path();
            Path.Stroke = Brushes.MidnightBlue;
            Path.StrokeThickness = 1;
            Path.Data = Line;
            ControlLineCanvas.canvas.Children.Add(Path);
            UpdateLine();
        }

        public void UpdateLine()
        {
            Point startPos = Start.GetPos();
            Point endPos = End.GetPos();
            double startX = startPos.X + Start.ActualWidth / 2;
            double startY = startPos.Y + Start.ActualHeight / 2;
            double endX = endPos.X + End.ActualWidth / 2;
            double endY = endPos.Y + End.ActualHeight / 2;
            Line.StartPoint = new Point(startX, startY);
            Line.EndPoint = new Point(endX, endY);
            StartPoint = Start.GetPixelPos();
            EndPoint = End.GetPixelPos();
        }
    }

    public class ControlLineThumb : Thumb
    {
        public ControlLine ControlLine { get; private set; }

        public ControlLineThumb(ControlLine controlLine, double x, double y)
        {
            MouseEnter += ControlLineThumb_MouseEnter;
            MouseLeave += ControlLineThumb_MouseLeave;
            ControlLine = controlLine;
        }

        private void ControlLineThumb_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void ControlLineThumb_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        public Point GetPos()
        {
            return new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
        }

        public Point GetPos(double horizontalChange, double verticalChange)
        {
            return new Point(Canvas.GetLeft(this) + horizontalChange, Canvas.GetTop(this) + verticalChange);
        }

        public Point GetPixelPos()
        {
            Point pos = GetPos();
            double canvasWidth = ControlLine.ControlLineCanvas.ActualWidth;
            double canvasHeight = ControlLine.ControlLineCanvas.ActualHeight;
            double pixelWidth = ControlLine.ControlLineCanvas.DirectBitmap.Width;
            double pixelHeight = ControlLine.ControlLineCanvas.DirectBitmap.Height;

            return new Point(pos.X * (pixelWidth / canvasWidth), pos.Y * (pixelHeight / canvasHeight));
        }

        public void SetPos(Point pos)
        {
            Canvas.SetLeft(this, pos.X);
            Canvas.SetTop(this, pos.Y);
        }
    }

    public class ControlLineStartThumb : ControlLineThumb
    {
        public ControlLineStartThumb(ControlLine controlLine, double x, double y) : base(controlLine, x, y)
        {
            ControlTemplate template = controlLine.ControlLineCanvas.FindResource("startTemplate") as ControlTemplate;
            Template = template;
            ApplyTemplate();
            DragDelta += ControlLineStartThumb_DragDelta;
            controlLine.ControlLineCanvas.canvas.Children.Add(this);
            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);
            Panel.SetZIndex(this, 1);
            UpdateLayout();
        }

        private void ControlLineStartThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            Canvas canvas = ControlLine.ControlLineCanvas.canvas;
            Point startPos = ControlLine.Start.GetPos(e.HorizontalChange, e.VerticalChange);
            Point endPos = ControlLine.End.GetPos();

            if (startPos.X < 0)
                startPos.X = 0;
            else if (startPos.X > canvas.ActualWidth - ControlLine.Start.ActualWidth)
                startPos.X = canvas.ActualWidth - ControlLine.Start.ActualWidth;
            if (startPos.Y < 0)
                startPos.Y = 0;
            else if (startPos.Y > canvas.ActualHeight - ControlLine.Start.ActualHeight)
                startPos.Y = canvas.ActualHeight - ControlLine.Start.ActualHeight;

            Vector v = Point.Subtract(startPos, endPos);
            ControlLine.Start.SetPos(startPos);
            ControlLine.Mid.SetPos(new Point(endPos.X + v.X / 2, endPos.Y + v.Y / 2));
            ControlLine.UpdateLine();
            thumb.UpdateLayout();
            e.Handled = true;
        }
    }
    public class ControlLineEndThumb : ControlLineThumb
    {
        public ControlLineEndThumb(ControlLine controlLine, double x, double y) : base(controlLine, x, y)
        {
            ControlTemplate template = controlLine.ControlLineCanvas.FindResource("endTemplate") as ControlTemplate;
            Template = template;
            ApplyTemplate();
            DragDelta += ControlLineEndThumb_DragDelta;
            controlLine.ControlLineCanvas.Add(this);
            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);
            Panel.SetZIndex(this, 1);
            UpdateLayout();
        }

        private void ControlLineEndThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            Canvas canvas = ControlLine.ControlLineCanvas.canvas;
            Point endPos = ControlLine.End.GetPos(e.HorizontalChange, e.VerticalChange);
            Point startPos = ControlLine.Start.GetPos();

            if (endPos.X < 0)
                endPos.X = 0;
            else if (endPos.X > canvas.ActualWidth - ControlLine.End.ActualWidth)
                endPos.X = canvas.ActualWidth - ControlLine.End.ActualWidth;
            if (endPos.Y < 0)
                endPos.Y = 0;
            else if (endPos.Y > canvas.ActualHeight - ControlLine.End.ActualHeight)
                endPos.Y = canvas.ActualHeight - ControlLine.End.ActualHeight;

            Vector v = Point.Subtract(endPos, startPos);
            ControlLine.End.SetPos(endPos);
            ControlLine.Mid.SetPos(new Point(startPos.X + v.X / 2, startPos.Y + v.Y / 2));
            ControlLine.UpdateLine();
            thumb.UpdateLayout();
            e.Handled = true;
        }
    }

    public class ControlLineMidThumb : ControlLineThumb
    {
        public ControlLineMidThumb(ControlLine controlLine, double x, double y) : base(controlLine, x, y)
        {
            ControlTemplate template = controlLine.ControlLineCanvas.FindResource("midTemplate") as ControlTemplate;
            Template = template;
            ApplyTemplate();
            DragDelta += ControlLineMidThumb_DragDelta;
            controlLine.ControlLineCanvas.Add(this);
            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);
            Panel.SetZIndex(this, 1);
            UpdateLayout();
        }

        private void ControlLineMidThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            Canvas canvas = ControlLine.ControlLineCanvas.canvas;
            Point startPos = ControlLine.Start.GetPos(e.HorizontalChange, e.VerticalChange);
            Point midPos = ControlLine.Mid.GetPos(e.HorizontalChange, e.VerticalChange);
            Point endPos = ControlLine.End.GetPos(e.HorizontalChange, e.VerticalChange);

            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;
            double startWidth = ControlLine.Start.ActualWidth;
            double startHeight = ControlLine.Start.ActualHeight;
            double endWidth = ControlLine.End.ActualWidth;
            double endHeight = ControlLine.End.ActualHeight;

            if (startPos.X < 0)
            {
                double diff = 0 - startPos.X;
                startPos.X += diff;
                midPos.X += diff;
                endPos.X += diff;
            }
            else if (startPos.X > canvasWidth - startWidth)
            {
                double diff = startPos.X - canvasWidth + startWidth;
                startPos.X -= diff;
                midPos.X -= diff;
                endPos.X -= diff;
            }

            if (startPos.Y < 0)
            {
                double diff = 0 - startPos.Y;
                startPos.Y += diff;
                midPos.Y += diff;
                endPos.Y += diff;
            }
            else if (startPos.Y > canvasHeight - startHeight)
            {
                double diff = startPos.Y - canvasHeight + startHeight;
                startPos.Y -= diff;
                midPos.Y -= diff;
                endPos.Y -= diff;
            }

            if (endPos.X < 0)
            {
                double diff = 0 - endPos.X;
                startPos.X += diff;
                midPos.X += diff;
                endPos.X += diff;
            }
            else if (endPos.X > canvasWidth - endWidth)
            {
                double diff = endPos.X - canvasWidth + endWidth;
                startPos.X -= diff;
                midPos.X -= diff;
                endPos.X -= diff;
            }

            if (endPos.Y < 0)
            {
                double diff = 0 - endPos.Y;
                startPos.Y += diff;
                midPos.Y += diff;
                endPos.Y += diff;
            }
            else if (endPos.Y > canvasHeight - endHeight)
            {
                double diff = endPos.Y - canvasHeight + endHeight;
                startPos.Y -= diff;
                midPos.Y -= diff;
                endPos.Y -= diff;
            }

            ControlLine.Start.SetPos(startPos);
            ControlLine.Mid.SetPos(midPos);
            ControlLine.End.SetPos(endPos);
            ControlLine.UpdateLine();
            thumb.UpdateLayout();
            e.Handled = true;
        }
    }
}
