using System;
using System.IO;
using System.Collections.Generic;
using CommandLine;
using SecretEngine;

namespace SECRET
{
    class Program
    {
        private static readonly License pLicense = new License(License.LicenseType.FULL, "68971edb60ee6f85c600bf8945b5130bdfabfa42");

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<EncryptOption, DecryptOption>(args).MapResult(
                (EncryptOption opts) => RunEncrypt(opts),
                (DecryptOption opts) => RunDecrypt(opts),
                errs => 1);
        }

        private static int RunEncrypt(EncryptOption option)
        {
            try
            {
                Crypto cryptoEngine = new Crypto(pLicense);
                CryptoKey key;
                bool isDir = false;
                string pass = GetPassword();

                if(pass == null)
                {
                    Console.WriteLine("Empty passwords not allowed.");
                    return 0;
                }

                Console.WriteLine();
                Console.WriteLine("Encrypting....");

                key = new CryptoKey(pass);

                //This is the only way to determine are we running --file or --directory?
                if (option.InputDirToEncrypt != null)
                    isDir = true;

                if (isDir)
                {
                    List<Tuple<bool, string>> files;
                    files = cryptoEngine.EncryptDirectory(option.InputDirToEncrypt, key, option.Recursive, option.Delete);

                    if (files.Count == 0)
                    {
                        Console.WriteLine("No files found in " + option.InputDirToEncrypt + ".");
                        return 0;
                    }

                    foreach (var t in files)
                    {
                        if (t.Item1 == false)
                        {
                            Console.WriteLine("Skipped file " + t.Item2 + ".");
                        }
                    }
                }
                else
                {
                    if (!cryptoEngine.EncryptFile(option.InputFileToEncrypt, key, option.Delete))
                        Console.WriteLine(cryptoEngine.LastErrorMessage);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return 0;
        }

        private static int RunDecrypt(DecryptOption option)
        {
            try
            {
                Crypto cryptoEngine = new Crypto(pLicense);
                CryptoKey key;
                bool isDir = false;
                string pass = GetPassword();

                if (pass == null)
                {
                    Console.WriteLine("Empty passwords not allowed.");
                    return 0;
                }

                Console.WriteLine();
                Console.WriteLine("Decrypting....");

                key = new CryptoKey(pass);
                //This is the only way to determine are we running --file or --directory?
                if (option.InputDirToDecrypt != null)
                    isDir = true;

                if (isDir)
                {
                    List<Tuple<bool, string>> files;
                    files = cryptoEngine.DecryptDirectory(option.InputDirToDecrypt, key, option.Recursive, option.Delete);

                    if (files.Count == 0)
                    {
                        Console.WriteLine("No files found in " + option.InputDirToDecrypt + ".");
                        return 0;
                    }

                    foreach (var t in files)
                    {
                        if (t.Item1 == false)
                        {
                            Console.WriteLine("Skipped file " + t.Item2 + ".");
                            Console.WriteLine(cryptoEngine.LastErrorMessage);
                        }
                    }
                }
                else
                {
                    if (!cryptoEngine.DecryptFile(option.InputFileToDecrypt, key, option.Delete))
                        Console.WriteLine(cryptoEngine.LastErrorMessage);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return 0;
        }

        private static string GetPassword()
        {
            string pass;

            pass = Environment.GetEnvironmentVariable("SECRET_MASTER_KEY");

            if (pass == null)
            {
                Console.Write("Password: ");

                do
                {
                    ConsoleKeyInfo oneKey = Console.ReadKey(true);

                    // Disable Backspace
                    if (oneKey.Key != ConsoleKey.Backspace && oneKey.Key != ConsoleKey.Enter)
                    {
                        pass += oneKey.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (oneKey.Key == ConsoleKey.Backspace && pass.Length > 0)
                        {
                            pass = pass.Substring(0, (pass.Length - 1));
                            Console.Write("\b \b");
                        }
                        else if (oneKey.Key == ConsoleKey.Enter)
                        {
                            break;
                        }
                    }

                } while (true);

            }

            if(pass == null)
            {
                return null;
            }

            //No spaces allowed
            return pass.Trim();
        }
    }
}
