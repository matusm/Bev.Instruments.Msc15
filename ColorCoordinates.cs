namespace Bev.Instruments.Msc15
{
    public class ColorCoordinates
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public double x => X / (X + Y + Z);
        public double y => Y / (X + Y + Z);
        public double uPrime => 4 * X / (X + 15 * Y + 3 * Z);
        public double vPrime => 9 * Y / (X + 15 * Y + 3 * Z);

        public ColorCoordinates()
        {
            X = double.NaN;
            Y = double.NaN;
            Z = double.NaN;
        }

        public ColorCoordinates(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
    }
}
