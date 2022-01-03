﻿using System;
using System.Runtime.InteropServices;
using System.Text;


namespace Bev.Instruments.Msc15
{
    public class Msc15 : IDisposable
    {
        public Msc15(string device)
        {
            GOMDMSC15_setPassword(passwordBev);
            GOMDMSC15_getHandle(device, out handle);
            ValidMeasurement = false;
            DeviceName = device;
        }

        public string DeviceName { get; }
        public string InstrumentManufacturer => "Gigahertz-Optik";
        public string InstrumentType => GetInstrumentType();
        public string InstrumentSerialNumber => $"{GetDeviceSerialNumber()}{GetDetectorSerialNumber()}";
        public string InstrumentFirmwareVersion => GetDeviceSoftwareVersion();
        public string InstrumentID => $"{InstrumentType} FW:{InstrumentFirmwareVersion} SN:{InstrumentSerialNumber}";
        public bool HasShutter => DeviceHasShutter();
        public double InternalTemperature => GetInternalTemperature();
        public bool ValidMeasurement { get; private set; }

        public double PhotopicValue => GetPhotopic();
        public double ScotopicValue => GetScotopic();
        public double CctValue => GetCCT();
        public double PeakWL => GetPeak();
        public double CentreWL => GetCentre();
        public double CentroidWL => GetCentroid();
        public double Fwhm => GetFwhm();

        public void Measure()
        {
            ValidMeasurement = false;
            GOMDMSC15_isOffsetInvalid(handle, out bool offsetIsInvalid);
            if (offsetIsInvalid)
            {
                if (HasShutter)
                    MeasureDark();
                else
                    return;
            }
            int rc = GOMDMSC15_measure(handle);
            if (rc < 0)
                return;
            ValidMeasurement = true;
        }

        public int MeasureDark()
        {
            //close shutter
            return GOMDMSC15_measureDarkOffset(handle);
            //open shutter
        }

        public void ActivateDynamicDarkMode()
        {
            GOMDMSC15_setDynamicDarkMode(handle, 1);
        }

        public void DeactivateDynamicDarkMode()
        {
            GOMDMSC15_setDynamicDarkMode(handle, 0);
        }

        public double GetLastIntegrationTime()
        {
            double value;
            int rc = GOMDMSC15_getLastIntegrationTime(handle, out value);
            if (rc < 0) return double.NaN;
            return value;
        }

        public SpectralValue[] GetNativeSpectrum()
        {
            SpectralValue[] spectrum = new SpectralValue[288];
            var wl = GetWLMapping();
            var ir = GetSpectralDataByPixel();
            for (int i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = new SpectralValue(wl[i], ir[i]);
            }
            return spectrum;
        }

        public SpectralValue[] GetVisSpectrum()
        {
            int nrOfSteps = 471;
            double[] values = new double[nrOfSteps];
            GOMDMSC15_getSpectralData(handle, 360, 1, nrOfSteps, values);
            SpectralValue[] spectrum = new SpectralValue[nrOfSteps];
            for (int i = 0; i < nrOfSteps; i++)
            {
                spectrum[i] = new SpectralValue(i+360, values[i]);
            }
            return spectrum;
        }

        private double[] GetWLMapping()
        {
            double[] values = new double[288];
            GOMDMSC15_getWLMapping(handle, values);
            return values;
        }

        private double[] GetSpectralDataByPixel()
        {
            double[] values = new double[288];
            GOMDMSC15_getSpectralDataByPixel(handle, values);
            return values;
        }

        private double GetInternalTemperature()
        {
            double value;
            GOMDMSC15_getTemperature(handle, out value);
            return value;
        }

        private double GetCCT()
        {
            if (!ValidMeasurement)
                return double.NaN;
            int rc = GOMDMSC15_getCCT(handle, out double value);
            if (rc < 0)
                return double.NaN;
            else
                return value;
        }

        private double GetPhotopic()
        {
            if (!ValidMeasurement)
                return double.NaN;
            int rc = GOMDMSC15_getPhotopic(handle, out double value);
            if (rc < 0)
                return double.NaN;
            else
                return value;
        }

        private double GetScotopic()
        {
            if (!ValidMeasurement)
                return double.NaN;
            int rc = GOMDMSC15_getScotopic(handle, out double value);
            if (rc < 0)
                return double.NaN;
            else
                return value;
        }

        private double GetPeak()
        {
            if (!ValidMeasurement)
                return double.NaN;
            int rc = GOMDMSC15_getPeakWL(handle, out double value);
            if (rc < 0)
                return double.NaN;
            else
                return value;
        }

        private double GetCentre()
        {
            if (!ValidMeasurement)
                return double.NaN;
            int rc = GOMDMSC15_getCentreWL(handle, out double value);
            if (rc < 0)
                return double.NaN;
            else
                return value;
        }

        private double GetCentroid()
        {
            if (!ValidMeasurement)
                return double.NaN;
            int rc = GOMDMSC15_getCentroidWL(handle, out double value);
            if (rc < 0)
                return double.NaN;
            else
                return value;
        }

        private double GetFwhm()
        {
            if (!ValidMeasurement)
                return double.NaN;
            int rc = GOMDMSC15_getFWHM(handle, out double value);
            if (rc < 0)
                return double.NaN;
            else
                return value;
        }

        private string GetInstrumentType()
        {
            int deviceType;
            int detecorType;
            GOMDMSC15_getMSC15DeviceType(handle, out deviceType);
            GOMDMSC15_getDetectorType(handle, out detecorType);
            if (detecorType == 0)
                return DeviceTypeToString(deviceType);
            return $"{DeviceTypeToString(deviceType)} + {DeviceTypeToString(detecorType)}";
        }

        private bool DeviceHasShutter()
        {
            int deviceType;
            int detecorType;
            GOMDMSC15_getMSC15DeviceType(handle, out deviceType);
            GOMDMSC15_getDetectorType(handle, out detecorType);
            return DeviceTypeHasShutter(deviceType) || DeviceTypeHasShutter(detecorType);
        }

        private bool DeviceTypeHasShutter(int type)
        {
            switch (type)
            {
                case 4:
                    return true;
                case 5:
                    return true;
                case 7:
                    return true;
                default:
                    return false;
            }
        }

        private string DeviceTypeToString(int type)
        {
            switch (type)
            {
                case 1:
                    return "MSC15";
                case 2:
                    return "MSC15-W";
                case 3:
                    return "LVMH-spectralux100";
                case 4:
                    return "CSS-45";
                case 5:
                    return "CSS-45-WT";
                case 6:
                    return "CSS-D";
                case 7:
                    return "CSS-45-HI";
                case 8:
                    return "MSC15-Bili";
                default:
                    return "<undefined>";
            }
        }

        private string GetDeviceSoftwareVersion()
        {
            double value;
            GOMDMSC15_getFirmwareVersion(handle, out value);
            return value.ToString();
        }

        private string GetDeviceSerialNumber()
        {
            StringBuilder sb = new StringBuilder(255);
            GOMDMSC15_getSerialNumber(handle, sb);
            return sb.ToString();
        }

        private string GetDetectorSerialNumber()
        {
            StringBuilder sb = new StringBuilder(255);
            GOMDMSC15_getDetectorSerialNumber(handle, sb);
            string sn = sb.ToString();
            if (string.IsNullOrWhiteSpace(sn))
                return string.Empty;
            return $"/{sn}";
        }


        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_setPassword(string password);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getHandle(string device, out int handle);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_releaseHandle(int handle);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_measure(int handle);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getCCT(int handle, out double value);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getPhotopic(int handle, out double value);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getScotopic(int handle, out double value);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getPeakWL(int handle, out double value);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getCentreWL(int handle, out double value);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getCentroidWL(int handle, out double value);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getFWHM(int handle, out double value);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_isOffsetInvalid(int handle, out bool value);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_measureDarkOffset(int handle);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getMSC15DeviceType(int handle, out int type);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getTemperature(int handle, out double value);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getDetectorType(int handle, out int type);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_setDynamicDarkMode(int handle, int mode);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getSerialNumber(int handle, StringBuilder sb);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getDetectorSerialNumber(int handle, StringBuilder sb);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getFirmwareVersion(int handle, out double value);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getLastIntegrationTime(int handle, out double value);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getWLMapping(int handle, double[] values);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getSpectralDataByPixel(int handle, double[] values);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getSpectralData(int handle, double startWl, double deltaWl, int nrOfSteps, double[] value);



        private const string passwordBev = "sdg4poiJ";
        private bool disposed = false;
        private int handle = 0;

        ~Msc15()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            if (!disposed)
            {
                GOMDMSC15_releaseHandle(handle);
                handle = -1;
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

    }
}
