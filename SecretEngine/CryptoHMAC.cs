using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SecretEngine
{
    internal static class CryptoHMAC
    {
        internal static readonly int HMAC_SIZE = 32;
        //Compute hmac hash for the file stream and write the hash value to
        //the end of the stream
        //In this use case, stream is assumed to be open and cursor in the end of the stream
        //We store the current end cursor position, rewind, calculate the stream and write the hash
        //back to the end of the stream
        internal static bool Sign(byte[] key, FileStream stream)
        {
            try
            {
                long offset = stream.Position;

                stream.Seek(0, SeekOrigin.Begin);

                using (HMACSHA256 hmac = new HMACSHA256(key))
                {
                    byte[] hash = hmac.ComputeHash(stream);
                    stream.Position = offset;
                    stream.Write(hash);
                }

                return true;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        internal static bool Verify(byte[] key, FileStream stream, byte[] oldHash)
        {
            try
            {
                bool retval = false;
                long offset = stream.Position;
                stream.Seek(0, SeekOrigin.Begin);

                using (HMACSHA256 hmac = new HMACSHA256(key))
                {
                    byte[] newHash = hmac.ComputeHash(stream);
                    uint diff = (uint)oldHash.Length ^ (uint)newHash.Length;

                    for (int i = 0; i < oldHash.Length && i < newHash.Length; i++)
                        diff |= (uint)(oldHash[i] ^ newHash[i]);

                    retval = diff == 0;
                }

                stream.Position = offset;

                return retval;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

    }
}
