namespace Bev.Instruments.Msc15
{
    public class ColorCoordinates
    {
        public double X { get; } = double.NaN;
        public double Y { get; } = double.NaN;
        public double Z { get; } = double.NaN;
        public double x => X / (X + Y + Z);
        public double y => Y / (X + Y + Z);
        public double uPrime => 4 * X / (X + 15 * Y + 3 * Z);
        public double vPrime => 9 * Y / (X + 15 * Y + 3 * Z);

        public ColorCoordinates() { } // for undefined value

        public ColorCoordinates(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
    }
}
