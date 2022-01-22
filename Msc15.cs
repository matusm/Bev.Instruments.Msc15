using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Bev.Instruments.Msc15
{
    public class Msc15 : IDisposable
    {
        private const string passwordBev = "sdg4poiJ";
        private bool disposed = false;
        private int handle = 0;

        public Msc15(string device)
        {
            GOMDMSC15_setPassword(passwordBev);
            DllVersion = GetDllVersion();
            GOMDMSC15_getHandle(device, out handle);
            ValidMeasurement = false;
            DeviceName = device;
        }

        public string DeviceName { get; }
        public string DllVersion { get; }
        public string InstrumentManufacturer => "Gigahertz-Optik";
        public string InstrumentType => GetInstrumentType();
        public string InstrumentSerialNumber => $"{GetDeviceSerialNumber()}{GetDetectorSerialNumber()}";
        public string InstrumentFirmwareVersion => GetDeviceSoftwareVersion();
        public string InstrumentID => $"{InstrumentType} FW:{InstrumentFirmwareVersion} SN:{InstrumentSerialNumber}";
        public double InternalTemperature => GetInternalTemperature();

        public int StatusNumber => GetStatus();
        public bool WarningStatus => StatusNumber > 0 ? true : false;
        public bool ErrorStatus => StatusNumber < 0 ? true : false;
        public bool StatusOK => StatusNumber == 0 ? true : false;
        public bool ValidMeasurement { get; private set; }
        public bool HasShutter => DeviceHasShutter();

        public double PhotopicValue => GetPhotopic();
        public double ScotopicValue => GetScotopic();
        public double CctValue => GetCCT();
        public double PeakWL => GetPeak();
        public double CentreWL => GetCentre();
        public double CentroidWL => GetCentroid();
        public double Fwhm => GetFwhm();
        public ColorCoordinates ColorValues => GetColorCoordinates();

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
            int rc = GOMDMSC15_getLastIntegrationTime(handle, out double value);
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
            GOMDMSC15_getTemperature(handle, out double value);
            return value;
        }

        private double ProveFunctionReturns(int rc, double v, bool forcePositive)
        {
            if (!ValidMeasurement) return double.NaN;
            if (rc < 0) return double.NaN;
            if (v < 0 && forcePositive) return double.NaN;
            return v;
        }

        private double GetCCT()
        {
            int rc = GOMDMSC15_getCCT(handle, out double value);
            return ProveFunctionReturns(rc, value, true);
        }

        private double GetPhotopic()
        {
            int rc = GOMDMSC15_getPhotopic(handle, out double value);
            return ProveFunctionReturns(rc, value, true);
        }

        private double GetScotopic()
        {
            int rc = GOMDMSC15_getScotopic(handle, out double value);
            return ProveFunctionReturns(rc, value, true);
        }

        private double GetPeak()
        {
            int rc = GOMDMSC15_getPeakWL(handle, out double value);
            return ProveFunctionReturns(rc, value, true);
        }

        private double GetCentre()
        {
            int rc = GOMDMSC15_getCentreWL(handle, out double value);
            return ProveFunctionReturns(rc, value, true);
        }

        private double GetCentroid()
        {
            int rc = GOMDMSC15_getCentroidWL(handle, out double value);
            return ProveFunctionReturns(rc, value, true);
        }

        private double GetFwhm()
        {
            int rc = GOMDMSC15_getFWHM(handle, out double value);
            return ProveFunctionReturns(rc, value, true);
        }

        private ColorCoordinates GetColorCoordinates()
        {
            if (ErrorStatus)
                return new ColorCoordinates();
            GOMDMSC15_getColor(handle, out double X, out double Y, out double Z);
            return new ColorCoordinates(X, Y, Z);
        }

        private string GetInstrumentType()
        {
            GOMDMSC15_getMSC15DeviceType(handle, out int deviceType);
            GOMDMSC15_getDetectorType(handle, out int detecorType);
            if (detecorType == 0)
                return DeviceTypeToString(deviceType);
            return $"{DeviceTypeToString(deviceType)} + {DeviceTypeToString(detecorType)}";
        }

        private int GetStatus()
        {
            GOMDMSC15_readStatus(handle, out int status);
            return status;
        }

        private bool DeviceHasShutter()
        {
            GOMDMSC15_getMSC15DeviceType(handle, out int deviceType);
            GOMDMSC15_getDetectorType(handle, out int detecorType);
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
                    return "LVMH-spectralux100"; // legacy product?
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
            GOMDMSC15_getFirmwareVersion(handle, out double value);
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

        private string GetDllVersion()
        {
            StringBuilder sb = new StringBuilder(255);
            GOMDMSC15_getDLLVersion(sb);
            return sb.ToString();
        }

        #region DLL imports

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_setPassword(string password);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getHandle(string device, out int handle);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_releaseHandle(int handle);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_measure(int handle);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_readStatus(int handle, out int status);

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
        private static extern int GOMDMSC15_getDLLVersion(StringBuilder sb);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getLastIntegrationTime(int handle, out double value);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getWLMapping(int handle, double[] values);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getSpectralDataByPixel(int handle, double[] values);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getSpectralData(int handle, double startWl, double deltaWl, int nrOfSteps, double[] value);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getColor(int handle, out double XValue, out double YValue, out double ZValue);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getColorCIE1931(int handle, out double xValue, out double yValue);

        [DllImport("GOMDMSC15.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GOMDMSC15_getColorCIE1976(int handle, out double uValue, out double vValue);

        #endregion

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
