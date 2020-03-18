using System;
using System.IO;

namespace HorizonCrypt.ConsoleApp
{
    internal static class Program
    {
        private const string Encrypt = "-c";
        private const string Decrypt = "-d";

        private static void Main(string[] args)
        {
            if (!TryExecute(args))
                PrintUsage();
            Console.Read();
        }

        private static bool TryExecute(string[] args)
        {
            if (args.Length < 2)
                return false;

            var type = args[0];
            var file = args[1];
            var directory = Path.GetDirectoryName(file);
            if (directory == null || !File.Exists(file))
                return false;

            var filename = Path.GetFileNameWithoutExtension(file);
            switch (type)
            {
                case Decrypt:
                {
                    var headerPath = Path.Combine(directory, filename + "Header.dat");
                    var headerData = File.ReadAllBytes(headerPath);
                    var encData = File.ReadAllBytes(file);

                    var decrypted = Encryption.Decrypt(headerData, encData);
                    File.WriteAllBytes(Path.Combine(directory, filename + "_decrypted.dat"), decrypted);
                    Console.WriteLine("Decrypted!");
                    return true;
                }

                case Encrypt:
                {
                    var decData = File.ReadAllBytes(file);
                    var seed = (uint)DateTime.Now.Ticks;
                    var (data, headerData) = Encryption.Encrypt(decData, seed);

                    File.WriteAllBytes(Path.Combine(directory, filename + "_encrypted.dat"), data);
                    File.WriteAllBytes(Path.Combine(directory, filename + "_encryptedHeader.dat"), headerData);
                    Console.WriteLine("Encrypted!");
                    return true;
                }

                default:
                    return false;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("HorizonCrypt by Cuyler");
            Console.WriteLine("Usage:");
            Console.WriteLine("\tHorizonCrypt [-c|-d] <input>");
        }
    }
}
