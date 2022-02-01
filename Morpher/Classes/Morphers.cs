using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace Morpher
{
    public class Morphers
    {
        private int numFrames;
        private List<Tuple<Point, Point>> sourceLines;
        private List<Tuple<Point, Point>> destLines;
        private List<Tuple<Vector, Vector>> deltas;
        private DirectBitmap source;
        private DirectBitmap dest;
        public double a { get; set; }
        private double b;
        private double p;
        private int width;
        private int height;

        public Morphers(ControlLineCanvas sourceCanvas, ControlLineCanvas destCanvas, int numFrames, double a, double b, double p)
        {
            this.numFrames = numFrames - 1;
            sourceLines = ControlLinesToPoints(sourceCanvas.ControlLines);
            destLines = ControlLinesToPoints(destCanvas.ControlLines);
            deltas = CalculateDeltas(sourceLines, destLines);
            source = sourceCanvas.DirectBitmap;
            dest = destCanvas.DirectBitmap;
            this.a = a;
            this.b = b;
            this.p = p;
            width = sourceCanvas.BitmapSource.PixelWidth;
            height = sourceCanvas.BitmapSource.PixelHeight;
        }

        public BitmapSource GenerateFrame(int frameIndex)
        {
            if (frameIndex == 0)
            {
                return source.BitmapSource;
            }
            else if (frameIndex == numFrames)
            {
                return dest.BitmapSource;
            }
            else if (frameIndex > numFrames)
            {
                return null;
            }

            DirectBitmap forwardWarp = Warp(source, sourceLines, frameIndex);
            DirectBitmap backwardWarp = Warp(dest, destLines, -(numFrames - frameIndex));
            DirectBitmap crossDisolve = CrossDisolve(forwardWarp, backwardWarp, frameIndex);

            return crossDisolve.BitmapSource;
        }

        private List<Tuple<Point, Point>> ControlLinesToPoints(List<ControlLine> controlLines)
        {
            List<Tuple<Point, Point>> points = new List<Tuple<Point, Point>>();
            foreach (var c in controlLines)
            {
                points.Add(new Tuple<Point, Point>(c.StartPoint, c.EndPoint));
            }
            return points;
        }

        private List<Tuple<Vector, Vector>> CalculateDeltas(List<Tuple<Point, Point>> sourceLines, List<Tuple<Point, Point>> destLines)
        {
            List<Tuple<Vector, Vector>> deltas = new List<Tuple<Vector, Vector>>();
            for (int i = 0; i < sourceLines.Count; i++)
            {
                Vector vec1 = Point.Subtract(destLines[i].Item1, sourceLines[i].Item1);
                Vector vec2 = Point.Subtract(destLines[i].Item2, sourceLines[i].Item2);
                vec1 = Vector.Divide(vec1, numFrames);
                vec2 = Vector.Divide(vec2, numFrames);
                deltas.Add(new Tuple<Vector, Vector>(vec1, vec2));
            }
            return deltas;
        }

        private DirectBitmap Warp(DirectBitmap bitmap, List<Tuple<Point, Point>> lines, int frameIndex)
        {
            DirectBitmap result = new DirectBitmap(width, height);

            for (int i = 0; i < source.Height; i++)
            {
                for (int j = 0; j < source.Width; j++)
                {
                    double totalWeight = 0;
                    double totalDeltaX = 0;
                    double totalDeltaY = 0;

                    Point t1 = new Point(j, i);

                    for (int k = 0; k < lines.Count; k++)
                    {
                        Tuple<Point, Point> line = lines[k];
                        Tuple<Vector, Vector> delta = deltas[k];

                        // Destination
                        Point p1 = Point.Add(line.Item1, delta.Item1 * frameIndex);
                        Point q1 = Point.Add(line.Item2, delta.Item2 * frameIndex);

                        Vector p1q1 = q1 - p1;
                        Vector n1 = new Vector(p1q1.Y, -(p1q1.X));
                        Vector t1p1 = p1 - t1;
                        Vector p1t1 = t1 - p1;

                        double d = (t1p1 * n1) / n1.Length;
                        double f = ((p1t1 * p1q1) / p1q1.Length) / p1q1.Length;

                        // Source
                        Point p2 = line.Item1;
                        Point q2 = line.Item2;

                        Vector p2q2 = q2 - p2;
                        Vector n2 = new Vector(p2q2.Y, -(p2q2.X));
                        n2 /= n2.Length;
                        Point fp = new Point(p2.X + f * p2q2.X, p2.Y + f * p2q2.Y);
                        Point t2 = new Point(fp.X - n2.X * d, fp.Y - n2.Y * d);

                        if (f < 0)
                        {
                            d = t1p1.Length;
                        }
                        else if (f > 1)
                        {
                            Vector t1q1 = t1 - q1;
                            d = t1q1.Length;
                        }

                        double w = Math.Pow(Math.Pow(p1q1.Length, p) / (a + Math.Abs(d)), b);
                        double deltaX = (t2.X - t1.X) * w;
                        double deltaY = (t2.Y - t1.Y) * w;

                        totalWeight += w;
                        totalDeltaX += deltaX;
                        totalDeltaY += deltaY;
                    }

                    Point sp = new Point(t1.X + (totalDeltaX / totalWeight), t1.Y + (totalDeltaY / totalWeight));

                    if (sp.X < 0) sp.X = 0;
                    else if (sp.X > source.Width) sp.X = source.Width - 1;
                    if (sp.Y < 0) sp.Y = 0;
                    else if (sp.Y > source.Height) sp.Y = source.Height - 1;

                    result.SetPixel((int)t1.X, (int)t1.Y, bitmap.GetPixel((int)sp.X, (int)sp.Y));
                }
            }

            return result;
        }

        private DirectBitmap CrossDisolve(DirectBitmap forwardWarp, DirectBitmap backwardWarp, int frameIndex)
        {
            DirectBitmap result = new DirectBitmap(width, height);

            for (int i = 0; i < source.Height; i++)
            {
                for (int j = 0; j < source.Width; j++)
                {
                    Color sourcePixel = forwardWarp.GetPixel(j, i);
                    Color destPixel = backwardWarp.GetPixel(j, i);
                    double sourceRatio = (double)(numFrames - frameIndex) / (numFrames);
                    double destRatio = (double)frameIndex / (numFrames);
                    int r = (int)((sourcePixel.R * sourceRatio) + (destPixel.R * destRatio));
                    int g = (int)((sourcePixel.G * sourceRatio) + (destPixel.G * destRatio));
                    int b = (int)((sourcePixel.B * sourceRatio) + (destPixel.B * destRatio));
                    if (r > 255) r = 255;
                    if (g > 255) g = 255;
                    if (b > 255) b = 255;
                    Color resultPixel = Color.FromArgb(r, g, b);
                    result.SetPixel(j, i, resultPixel);
                }
            }

            return result;
        }
    }
}
