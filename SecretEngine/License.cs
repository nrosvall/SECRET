using System;
using System.Text.RegularExpressions;

namespace SecretEngine
{
    public class License
    {
        private bool pIsValid;

        public License(string code)
        {
            pIsValid = false;
            Validate(code);
        }

        internal bool IsValid
        {
            get { return pIsValid; }
        }

        private void Validate(string code)
        {
            if (code.Length != 40)
                pIsValid = false;

            pIsValid = (new Regex("^[a-fA-F0-9]+$").IsMatch(code));
        }
    }
}
