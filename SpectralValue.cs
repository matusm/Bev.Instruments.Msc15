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
