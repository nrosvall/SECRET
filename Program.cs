using System;
using System.Collections.Generic;
using CommandLine;
using SecretEngine;

namespace SECRET
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<EncryptOption, DecryptOption>(args).MapResult(
                (EncryptOption opts) => RunEncrypt(opts),
                (DecryptOption opts) => RunDecrypt(opts),
                errs => 1);
        }

        private static int RunEncrypt(EncryptOption option)
        {
            Crypto cryptoEngine = new Crypto();
            CryptoKey key;
            bool isDir = false;
            string pass = GetPassword();

            Console.WriteLine();
            Console.WriteLine("Encrypting....");

            key = new CryptoKey(pass);

            //This is the only way to determine are we running --file or --directory?
            if (option.InputDirToEncrypt != null)
                isDir = true;
            
            if(isDir)
            {
                List<Tuple<bool, string>> files;
                files = cryptoEngine.EncryptDirectory(option.InputDirToEncrypt, key, option.Recursive, option.Delete);

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

            return 0;
        }

        private static int RunDecrypt(DecryptOption option)
        {
            Crypto cryptoEngine = new Crypto();
            CryptoKey key;
            bool isDir = false;
            string pass = GetPassword();

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
                 
                foreach(var t in files)
                {
                    if(t.Item1 == false)
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

            return 0;
        }

        private static string GetPassword()
        {
            string pass = "";
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

            //No spaces allowed
            return pass.Trim();
        }
    }
}
