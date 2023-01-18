//*****************************************************************************
//
// Class to handle a spectrum obtained from supported instruments.
//
// Two types of spectra are supported: native spectra (based on sensor pixels)
// and VIS spectra ranging from 360 nm to 830 nm in 1 nm intervalls.
// The type is automatically determined on instantiation of the object.
// The main usage of this class is the possibility to correct the spectral
// irradiance values obtained by a supported instrument.
// To this end the resposivity spectrum of the very instrument must be
// provided by the user. The instrument ID is a property of the respective
// object, and only matching IDs (for the raw spectrum and responsivity)
// are treated. 
//
// This class has no other functionality.
//
// Usage:
//   1) instantiate class with an array of SpectralValues and the
//      instrument ID (a string);
//   2) one can consume some properties;
//   3) the provided spectrum can be corrected using a mating responivity
//      spectrum by calling GetCorrectedSpectrum();
//   4) if not successful a spectrum of "SpectrumType.Invalid" is returned;
//   5) all properties are getters only.
//
//*****************************************************************************

using System;
using System.Linq;

namespace Bev.Instruments.Msc15
{
    public class Msc15Spectrum
    {
        private const int nativeLength = 288; // pixel number of sensor
        private const int visLength = 471; // 360 nm to 830 nm
        private SpectralValue[] rawSpectrum;

        public string SpectrometerID { get; } = string.Empty;
        public DateTime CreationDate { get; }
        public SpectrumType Type { get; } = SpectrumType.Invalid;
        public SpectralValue[] RawSpectrum => rawSpectrum;
        public int Length => rawSpectrum.Length;
        private Msc15Spectrum InvalidSpectrum => new Msc15Spectrum();

        public Msc15Spectrum() { } // creates an invalid spectrum

        public Msc15Spectrum(SpectralValue[] raw, string id)
        {
            rawSpectrum = raw;
            SpectrometerID = id;
            CreationDate = DateTime.UtcNow;
            Type = EstimateSpectrumType();
        }

        public Msc15Spectrum GetCorrectedSpectrum(Msc15Spectrum responsivity)
        {
            if (responsivity.SpectrometerID != SpectrometerID)
                return InvalidSpectrum;
            if (responsivity.Type != Type)
                return InvalidSpectrum;
            SpectralValue[] correctedSpectrumValues = new SpectralValue[responsivity.Length];
            for (int i = 0; i < responsivity.Length; i++)
            {
                double wavelength = rawSpectrum[i].Wavelength;
                double irradiance = rawSpectrum[i].Irradiance * responsivity.RawSpectrum[i].Irradiance;
                correctedSpectrumValues[i] = new SpectralValue(wavelength, irradiance);
            }
            // modify ID to avoid multiple correction
            string newID = $"{SpectrometerID}.corrected";
            Msc15Spectrum correctedSpectrum = new Msc15Spectrum(correctedSpectrumValues, newID);
            return correctedSpectrum;
        }

        private SpectrumType EstimateSpectrumType()
        {
            if (rawSpectrum.Length == nativeLength)
            {
                if (rawSpectrum.Last().Wavelength - rawSpectrum.First().Wavelength < 400)
                {
                    return SpectrumType.Unknown;
                }
                return SpectrumType.Native;
            }
            if (rawSpectrum.Length == visLength)
            {
                if (rawSpectrum.Last().Wavelength - rawSpectrum.First().Wavelength < visLength)
                {
                    return SpectrumType.Unknown;
                }
                return SpectrumType.VIS;
            }
            return SpectrumType.Unknown;
        }

    }

    public enum SpectrumType
    {
        Unknown,
        VIS,
        Native,
        Invalid
    }
}
