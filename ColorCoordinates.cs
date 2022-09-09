//*****************************************************************************
//
// Class as a container for color coordinates.
//
// Given the tristimulus values X, Y, Z the chromaticity coordinates x, y and
// u' and v' are provided as computed properties. 
//
// This class has no other functionality.
//
// Usage:
//   1) instantiate class with the three tristimulus values;
//   2) instantiating the empty constructor creates an invalid object;
//   3) consume them and derived values as properties;
//   4) all properties are getters only.
//
//*****************************************************************************

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
