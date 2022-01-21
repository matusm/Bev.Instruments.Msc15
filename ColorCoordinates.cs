using System;
namespace Bev.Instruments.Msc15
{
    public class ColorCoordinates
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }
        public double x { get; private set; }
        public double y { get; private set; }
        public double u { get; private set; }
        public double v { get; private set; }

        public ColorCoordinates()
        {
            Invalidate();
        }

        public ColorCoordinates(double X, double Y, double Z, double x, double y, double u, double v)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.x = x;
            this.y = y;
            this.u = u;
            this.v = v;
        }

        private void Invalidate()
        {
            X = double.NaN;
            Y = double.NaN;
            Z = double.NaN;
            x = double.NaN;
            y = double.NaN;
            u = double.NaN;
            v = double.NaN;
        }
    }
}
