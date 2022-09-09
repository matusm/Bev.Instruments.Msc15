//*****************************************************************************
//
// Class as a container for a single spectral value.
//
// A spectral value is comprised of a wavelength and a corresponding value.
// For the application of this library this value is usually a spectral
// irradiance (W/m^3) or a dimensionless responsivity factor.
//
// This class has no other functionality.
//
// Usage:
//   1) instantiate class with wavelength and irradiance as parameters;
//   2) consume the values as properties;
//   3) all properties are getters only.
//
//*****************************************************************************

namespace Bev.Instruments.Msc15
{
    public struct SpectralValue
    {
        public SpectralValue(double wavelength, double irradiance)
        {
            Wavelength = wavelength;
            Irradiance = irradiance;
        }

        public double Wavelength { get; }
        public double Irradiance { get; }

        public override string ToString() => $"{Wavelength}, {Irradiance}";
    }
}
