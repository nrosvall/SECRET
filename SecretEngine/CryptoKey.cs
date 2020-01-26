using System;
using System.Security.Cryptography;

namespace SecretEngine
{
    public class CryptoKey
    {
        private readonly byte[] pPasswordBytes;
        private readonly int pKeyIterations;
        private readonly RandomDataBytes pDataByteGenerator;
        internal readonly static int SALT_SIZE = 32; //256 bits;
        private readonly static int KEY_SIZE = 32;
        private readonly static int KEY_ITERATIONS = 200000;
        private readonly Tuple<byte[], byte[]> pKeyData;
        private readonly int plengthInBytes;

        public CryptoKey(string password)
        {
            pPasswordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            pKeyIterations = KEY_ITERATIONS;
            plengthInBytes = KEY_SIZE;
            pDataByteGenerator = new RandomDataBytes();

            pKeyData = GetNewKeyData();
        }

        private Tuple<byte[], byte[]> GetNewKeyData()
        {
            byte[] salt = pDataByteGenerator.GenerateRandomBytes(SALT_SIZE);
            byte[] key;

            Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(pPasswordBytes, salt, pKeyIterations);
            key = k1.GetBytes(plengthInBytes);

            k1.Reset();

            return Tuple.Create(key, salt);
        }

        public Tuple<byte[], byte[]> KeyData
        {
            get { return pKeyData; }
        }

        public byte[] GetKeyBytesFromSalt(byte[] salt)
        {
            Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(pPasswordBytes, salt, pKeyIterations);
            byte[] key;

            key = k1.GetBytes(plengthInBytes);

            k1.Reset();

            return key;
        }
    }
}
