//Copyright 2021 Niko Rosvall <niko@byteptr.com>

using System;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SecretEngine
{
    public class Crypto
    {
        private readonly RandomDataBytes pDataByteGenerator;

        private const int IV_SIZE = 16; //128 bits
        private const string CRYPTO_FILE_EXT = ".secret";
        private readonly byte[] pMagic;
        private string pLastErrorMessage;

        public Crypto()
        {
            pDataByteGenerator = new RandomDataBytes();
            pMagic = BitConverter.GetBytes(0x736563726574);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(pMagic);
        }

        public string LastErrorMessage
        {
            get { return pLastErrorMessage; }
        }

        public bool EncryptFile(string file, CryptoKey key, bool deletePlain = false)
        {
            FileStream fsOut = null;
            FileStream fsIn = null;
            CryptoStream cryptoStream = null;

            try
            {
                byte[] iv = pDataByteGenerator.GenerateRandomBytes(IV_SIZE);

                //item1 = key, item2 = salt
                Tuple<byte[], byte[]> keyData = key.KeyData;

                byte[] keySalt = keyData.Item2;

                AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

                aes.Key = keyData.Item1;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;

                fsIn = new FileStream(file, FileMode.Open);

                bool isAlreadySecretFile = false;
                byte[] magic = new byte[pMagic.Length];
                fsIn.Read(magic, 0, pMagic.Length);

                if (magic.Length == pMagic.Length)
                {
                    for (int i = 0; i < magic.Length; i++)
                    {
                        if (magic[i] != pMagic[i])
                        {
                            //We can break and continue with the encryption as the file is not encrypted with Secret
                            //or at least does not have the same magic
                            isAlreadySecretFile = false;
                            break;
                        }
                        else
                        {
                            isAlreadySecretFile = true;
                        }
                    }
                }

                if(isAlreadySecretFile)
                    throw new Exception("File " + file + " already encrypted with Secret. Skipping.");

                fsIn.Position = 0;

                fsOut = new FileStream(file + CRYPTO_FILE_EXT, FileMode.CreateNew);

                fsOut.Write(pMagic, 0, pMagic.Length);
                fsOut.Write(iv, 0, iv.Length);
                fsOut.Write(keySalt, 0, keySalt.Length);

                cryptoStream = new CryptoStream(fsOut, aes.CreateEncryptor(), CryptoStreamMode.Write);


                byte[] buffer = new byte[4096];
                int read;

                while((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    cryptoStream.Write(buffer, 0, read);
                }

                cryptoStream.FlushFinalBlock();

                fsIn.Close();

                CryptoHMAC.Sign(keyData.Item1, fsOut);

                cryptoStream.Close();
                fsOut.Close(); 

                if (deletePlain)
                    File.Delete(file);

                return true;
            }
            catch(Exception ex)
            {
                if (fsIn != null)
                    fsIn.Close();
                if (cryptoStream != null)
                    cryptoStream.Close();
                if (fsOut != null)
                    fsOut.Close();

                pLastErrorMessage = ex.Message;
                return false;
            }
           
        }

        public List<Tuple<bool, string>> EncryptDirectory(string dir, CryptoKey key, bool recursive = false, bool deletePlain = false)
        {
            List<Tuple<bool, string>> fileList = new List<Tuple<bool, string>>();

            try
            {
                IEnumerable<string> files = Directory.EnumerateFiles(dir, "*",
                   recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    bool success = EncryptFile(file, key, deletePlain);
                    fileList.Add(Tuple.Create(success, file));
                }
            }
            catch(Exception ex)
            {
                pLastErrorMessage = ex.Message;
            }

            return fileList;
        }

        public bool DecryptFile(string file, CryptoKey key, bool deleteEncrypted=false)
        {
            FileStream fsOut = null;
            FileStream fsIn = null;
            CryptoStream decryptoStream = null;
            string tmpFilePath = Path.GetTempFileName();
            FileStream fsInTmp = new FileStream(tmpFilePath, FileMode.Open);

            try
            {
                fsIn = new FileStream(file, FileMode.Open);
                
                //Make a backup copy of the encrypted file stream
                //This because if we fail after calling SetLength for the stream
                //we can restore the original encrypted file on failure using this stream copy.
                fsIn.CopyTo(fsInTmp);
                fsInTmp.Position = 0;

                byte[] magic = new byte[pMagic.Length];
                byte[] iv = new byte[IV_SIZE];
                byte[] salt = new byte[CryptoKey.SALT_SIZE];
                byte[] hmac = new byte[CryptoHMAC.HMAC_SIZE];

                fsIn.Seek(-CryptoHMAC.HMAC_SIZE, SeekOrigin.End);
                fsIn.Read(hmac, 0, CryptoHMAC.HMAC_SIZE);
                fsIn.Seek(0, SeekOrigin.Begin);

                //Set the length for the stream without the hmac
                fsIn.SetLength(Math.Max(0, fsIn.Length - hmac.Length));

                fsIn.Read(magic, 0, pMagic.Length);

                if (magic.Length != pMagic.Length)
                {
                    throw new Exception("File " + file + " header does not match. Skipping");
                }

                for (int i = 0; i < magic.Length; i++) {
                    if (magic[i] != pMagic[i])
                    {
                        throw new Exception("File " + file + " not encrypted with Secret. Skipping.");
                    }
                }

                fsIn.Read(iv, 0, IV_SIZE);
                fsIn.Read(salt, 0, CryptoKey.SALT_SIZE);

                byte[] keyData = key.GetKeyBytesFromSalt(salt);

                if(!CryptoHMAC.Verify(keyData, fsIn, hmac))
                {
                    throw new Exception("Data tampered or invalid password. Abort.");
                }

                AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

                aes.Key = keyData;              
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;

                file = file.Substring(0, file.Length - CRYPTO_FILE_EXT.Length);

                fsOut = new FileStream(file, FileMode.Create);

                decryptoStream = new CryptoStream(fsOut, aes.CreateDecryptor(), CryptoStreamMode.Write);

                byte[] buffer = new byte[4096];
                int read;

                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0 )
                {
                decryptoStream.Write(buffer, 0, read);  
                }

                decryptoStream.FlushFinalBlock();

                if(!deleteEncrypted)
                {
                    //As we do not delete the encrypted file, restore the original in place.
                    fsIn.Position = 0;
                    fsInTmp.CopyTo(fsIn);
                }

                fsIn.Close();
                fsInTmp.Close();
                File.Delete(tmpFilePath);
                decryptoStream.Close();
                fsOut.Close();

                if(deleteEncrypted)
                {
                    //As we stripped the extension from the inputfile, append it back to delete the encrypted file
                    File.Delete(file + CRYPTO_FILE_EXT);
                }

                return true;
            }
            catch (Exception ex)
            {
                //Restore the original file, just in case SetLength() was called for it.
                if (fsIn != null)
                {
                    fsIn.Position = 0;
                    fsInTmp.CopyTo(fsIn);
                }

                fsInTmp.Close();

                File.Delete(tmpFilePath);

                if (fsIn != null)
                    fsIn.Close();
                if (decryptoStream != null)
                    decryptoStream.Close();
                if (fsOut != null)
                    fsOut.Close();

                pLastErrorMessage = ex.Message;
                return false;
            }
        }

        public List<Tuple<bool, string>> DecryptDirectory(string dir, CryptoKey key, bool recursive = false, bool deleteEncrypted = false)
        {
            List<Tuple<bool, string>> fileList = new List<Tuple<bool, string>>();

            try
            {
                IEnumerable<string> files = Directory.EnumerateFiles(dir, "*" + CRYPTO_FILE_EXT,
                   recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    bool success = DecryptFile(file, key, deleteEncrypted);
                    fileList.Add(Tuple.Create(success, file));
                }
            }
            catch(Exception ex)
            {
                pLastErrorMessage = ex.Message;
            }

            return fileList;
        }

    }
}
                                                                                                                                                                                                                                                                                         