using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using HalconDotNet;
using VistaCrypto;
using Foerster.Models.System;
using Application = System.Windows.Application;

namespace Foerster.Models.Managers
{
    public class LicenseManager
    {
        #region Fields
        // Singleton
        private static LicenseManager? _instance = null;
        private static readonly object _lock = new object();
        private XmlDocument _licenseFile;
        // License loading
        private string _licenseRootDirectory = "";
        private string _licenseFileName = "vista_license.xml";
        // License validation
        private string _generatorPublicKey = "NTEyITxSU0FLZXlWYWx1ZT48TW9kdWx1cz51UENHd3FvR29XS09OUmJjTi85Y2lmS29DQ1BVNGVtZDA0RVdSUjRkSHJLRkVwQkxQQmRpdkdnK1E2bG5JYkdZUUFuQ3VqVVdTM21rNXFNS2kxNngwUT09PC9Nb2R1bHVzPjxFeHBvbmVudD5BUUFCPC9FeHBvbmVudD48L1JTQUtleVZhbHVlPg==";
        private static Timer _validationTimer;
        private int _timerPeriod;
        #endregion

        #region Properties
        // Singleton
        public static LicenseManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new LicenseManager();
                    }
                    return _instance;
                }
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the instance of the <c>LicenseManager</c>.
        /// </summary>
        protected LicenseManager()
        {
            _licenseFile = new XmlDocument();
            _licenseRootDirectory = Path.Combine(Environment.CurrentDirectory, "License");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Reads the license key from the XML file and uses the license generator's public key to validate it.
        /// If the license is not valid, notifies the user and shuts down the application.
        /// </summary>
        public void ValidateLicense()
        {
            // TO-DO: REMOVE THIS IF STATEMENT BEFORE RELEASE
            if (SystemConfiguration.Instance.RunMode == RunMode.online)
            {
                string licensePath = Path.Combine(_licenseRootDirectory, _licenseFileName);
                string licenseErrorCode = string.Empty;
                // Load the license file
                if (!File.Exists(licensePath))
                {
                    licenseErrorCode = "404";
                    ThrowLicenseError(licenseErrorCode);
                    return;
                }
                _licenseFile.Load(Path.Combine(_licenseRootDirectory, _licenseFileName));
                // Extract the signed content
                string licenseKey = _licenseFile.SelectSingleNode("VistaLicense/Key").InnerText;
                licenseErrorCode = CheckLicenseKey(licenseKey);
                if (licenseErrorCode != string.Empty)
                {
                    ThrowLicenseError(licenseErrorCode);
                }
            }
        }
        /// <summary>
        /// Callback function to validate the license upon timer's timeout
        /// </summary>
        /// <param name="stateInfo">State info. Assumed to be null in the timer's configuration.</param>
        public void ValidateLicense(Object stateInfo)
        {
            ValidateLicense();
            _validationTimer.Change(_timerPeriod, Timeout.Infinite);
            
        }
        /// <summary>
        /// Sets a periodic validation mechanism to validate the vista license.
        /// </summary>
        /// <param name="timerPeriod">Interval between license validations in milliseconds.</param>
        public void SetPeriodicValidation(int timerPeriod)
        {
            _timerPeriod = timerPeriod;
            _validationTimer = new Timer(new TimerCallback(ValidateLicense), null, timerPeriod, Timeout.Infinite);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets the user's drive primary serial number by creating, executing and accessing the output stream of a <c>Process</c> instance.
        /// </summary>
        /// <returns>A string representing the user's primary drive serial number.</returns>
        private string GetPrimaryDriveSerialNumber()
        {
            // Set and run a process to query the C volume's serial number
            Process queryProcess = new();
            queryProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            queryProcess.StartInfo.RedirectStandardOutput = true;
            queryProcess.StartInfo.UseShellExecute = false;
            queryProcess.StartInfo.FileName = "cmd.exe";
            queryProcess.StartInfo.Arguments = "/C vol";
            queryProcess.Start();
            // Parse the process' outputs
            string serialNumber = queryProcess.StandardOutput.ReadToEnd();
            serialNumber = serialNumber.Substring(serialNumber.LastIndexOf(' ') + 1, 9);

            return serialNumber;
        }

        /// <summary>
        /// Show a message box to notify license errors and shut down the application.
        /// </summary>
        /// <param name="errorType">The error code in strintg format.</param>
        private void ThrowLicenseError(string errorCode)
        {
            // Get the user's primary drive serial number and dongle ID
            string primaryDriveIdMessageString = $"\nUser ID: {GetPrimaryDriveSerialNumber()}";
            HTuple dongleId;
            HTuple versionInfo;
            try
            {
                HOperatorSet.GetSystem("licensed_hostid", out dongleId);
                // Get the number of the HALCON library
                HOperatorSet.GetSystem("file_version", out versionInfo);
            }
            catch (HalconException halconExeption)
            {
                dongleId = "null";
                versionInfo = "null";
            }
            string halconIdMessageString = $"\nHalcon ID: {dongleId.ToString().Trim('"')}";

            string version = $"\n{versionInfo.ToString().Trim('"')}";
            // Get the error message in terms of the current value of the licenseErrorCode
            string licenseErrorMessage = GetLicenseErrorMessage(errorCode);
            string errorCodeString = $"\n\nError code: {errorCode}";
            // Notify the user
            licenseErrorMessage += " Please contact tech support for assistance." + $"{errorCodeString}" + primaryDriveIdMessageString + halconIdMessageString;
            Application.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                MessageBox.Show(licenseErrorMessage, "License error - Vista AI Platform", MessageBoxButton.OK, MessageBoxImage.Warning);
            });
            Environment.Exit(0);
        }

        /// <summary>
        /// Get the error message that corresponds to the given error code.
        /// </summary>
        /// <param name="errorCode">The error code in string format.</param>
        /// <returns></returns>
        private string GetLicenseErrorMessage(string errorCode)
        {
            string licenseErrorMessage = string.Empty;
            switch (errorCode)
            {
                case "404":
                    licenseErrorMessage =  "License not found.";
                    break;
                case "401":
                    licenseErrorMessage = "Invalid license.";
                    break;
                case "402":
                    licenseErrorMessage = "Halcon HostId not found.";
                    break;
                default:
                    licenseErrorMessage = "Unknown license error.";
                    break;
            }

            return licenseErrorMessage;
        }

        /// <summary>
        /// Compares the current and provided target license Ids.
        /// </summary>
        /// <param name="licenseKey">The license key.</param>
        /// <returns>The license error code in string format. The string is empty for valid license.</returns>
        private string CheckLicenseKey(string licenseKey)
        {
            HTuple currentHostId;
            try
            {
                currentHostId = HSystem.GetSystem("licensed_hostid");
            }
            catch(HalconException halconException)
            {
                currentHostId = "unknown";
            }
            string licenseErrorCode = string.Empty;
            // License not found error
            if (currentHostId == "unknown")
            {
                licenseErrorCode = "402";
            }
            else
            {
                // Check that licenses match
                bool checkPass = RSA_Sign.VerifySignedText(currentHostId, licenseKey, _generatorPublicKey);
                if (!checkPass)
                {
                    licenseErrorCode = "401";
                }
            }

            return licenseErrorCode;
        }
        #endregion

    }
}