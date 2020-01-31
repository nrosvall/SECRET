using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SecretEngine
{
    public class License
    {
        public enum LicenseType
        {
            TRIAL,
            FULL
        }

        private bool pIsValid;

        public License(LicenseType type, string code)
        {
            pIsValid = false;

            if (type == LicenseType.TRIAL)
            {
                ValidateTrial();
            }
            else if (type == LicenseType.FULL)
            {
                Validate(code);
            }
            else
            {
                throw new Exception("Invalid license type.");
            }
        }

        internal bool IsValid
        {
            get { return pIsValid; }
        }

        private void Validate(string code)
        {
            if (code == null)
            {
                pIsValid = false;
                return;
            }
            if (code.Length != 40)
            {
                pIsValid = false;
                return;
            }

            pIsValid = (new Regex("^[a-fA-F0-9]+$").IsMatch(code));
        }

        private bool CheckForValidTrial(string path)
        {
            bool retval;

            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path += "/trial";

                //If the trial file does not exist, create a new one
                if (!File.Exists(path))
                {
                    string time = DateTime.Now.ToString("yyyy-MM-dd");
                    File.Create(path).Close();

                    StreamWriter w = new StreamWriter(path);
                    w.WriteLine(time);
                    w.Close();
                }

                StreamReader r = new StreamReader(path);
                string timeValue = r.ReadLine();
                r.Close();
                DateTime datetimeValue = DateTime.Parse(timeValue);
                DateTime currentDatetimeValue = DateTime.Now;

                double total = (currentDatetimeValue - datetimeValue).TotalDays;
                if (total > 30)
                    retval = false;

                retval = true;
            }
            catch
            {               
                retval = false;
            }

            return retval;
        }

        private void ValidateTrial()
        {
            string path;

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = Environment.GetEnvironmentVariable("LOCALAPPDATA") + "\\SecretEngine";
            }
            else
            {
                //we're running either on Linux or MacOS
                path = Environment.GetEnvironmentVariable("HOME") + "/.SecretEngine";
            }

            pIsValid = CheckForValidTrial(path);
        }
    }
}
