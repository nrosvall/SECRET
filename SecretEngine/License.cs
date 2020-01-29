using System;
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

        private void ValidateTrial()
        {

        }
    }
}
