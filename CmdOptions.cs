using System;
using System.IO;
using CommandLine;

namespace SECRET
{
    internal static class Helper
    {
        internal static string EscapeInputPath(string inputPath)
        {
            if (inputPath == null)
                return null;

            string safePath = inputPath.Replace("\"", Path.DirectorySeparatorChar.ToString());
            safePath = safePath.TrimEnd(Path.DirectorySeparatorChar);

            return safePath;
        }
    }

    [Verb("encrypt", HelpText = "Encrypt files or directories.")]
    internal class EncryptOption
    {
        [Option('f', "file", Required = true, HelpText = "File to be encrypted.", SetName = "file")]
        public string InputFileToEncrypt { get; set; }

        [Option('d', "directory", Required = true, HelpText = "Directory to be encrypted.", SetName="dir")]
        public string InputDirToEncrypt { get; set; }

        [Option('r', "recursive", Default = false, HelpText = "Recursive encryption, only used with --directory", 
            SetName="dir")]
        public bool Recursive { get; set; }

        [Option(Default = false, HelpText = "Delete original file(s).")]
        public bool Delete { get; set; }

    }

    [Verb("decrypt",  HelpText = "Decrypt files or directories.")]
    internal class DecryptOption
    {
        [Option('f', "file", Required=true, HelpText = "File to be decrypted.", SetName = "file")]
        public string InputFileToDecrypt { get; set; }

        [Option('d', "directory", Required = true, HelpText = "Directory to be decrypted.", SetName = "dir")]
        public string InputDirToDecrypt { get; set; }

        [Option('r', "recursive", Default = false, HelpText = "Recursive decryption, only used with --directory", 
            SetName = "dir")]
        public bool Recursive { get; set; }

        [Option(Default = false, HelpText = "Delete original file(s).")]
        public bool Delete { get; set; }
    }
}
