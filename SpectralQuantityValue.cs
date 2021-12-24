using At.Matus.StatisticPod;

namespace Bev.Instruments.Msc15
{
    public class SpectralQuantityValue
    {
        public double AverageValue => valuePod.AverageValue;
        public double StandardDeviation => valuePod.StandardDeviation;
        public double Wavelength { get; private set; }

        private StatisticPod valuePod = new StatisticPod();

        public void UpdateValue(SpectralValue sv)
        {
            UpdateValue(sv.Irradiance);
            Wavelength = sv.Wavelength;
        }

        public void UpdateValue(double value)
        {
            valuePod.Update(value);
        }
    }
}
