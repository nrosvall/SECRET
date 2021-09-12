//Copyright 2021 Niko Rosvall <niko@byteptr.com>

using System;
using System.Security.Cryptography;

namespace SecretEngine
{
    internal class RandomDataBytes
    {
        private readonly RNGCryptoServiceProvider pRng;

        internal RandomDataBytes()
        {
            pRng = new RNGCryptoServiceProvider();
        }

        ~RandomDataBytes()
        {
            pRng.Dispose();
        }

        internal byte[] GenerateRandomBytes(int length)
        {
            byte[] data = new byte[length];

            pRng.GetBytes(data);

            return data;
        }
    }
}
