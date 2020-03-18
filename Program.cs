using System;
using System.IO;

namespace HorizonCrypt
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args?.Length < 2 || (args[0] != "-c" && args[0] != "-d") || !File.Exists(args[1]))
            {
                PrintUsage();
                return;
            }

            if (args[0] == "-d")
            {
                var directory = Path.GetDirectoryName(args[1]);
                var header = Path.Combine(directory, Path.GetFileNameWithoutExtension(args[1]) + "Header.dat");
                var data = Encryption.Decrypt(File.ReadAllBytes(header), File.ReadAllBytes(args[1]));
                File.WriteAllBytes(Path.Combine(directory, Path.GetFileNameWithoutExtension(args[1]) + "_decrypted.dat"), data);
            }
            else
            {
                var directory = Path.GetDirectoryName(args[1]);
                var (data, headerData) = Encryption.Encrypt(File.ReadAllBytes(args[1]));
                
                File.WriteAllBytes(Path.Combine(directory, Path.GetFileNameWithoutExtension(args[1]) + "_encrypted.dat"), data);
                File.WriteAllBytes(Path.Combine(directory, Path.GetFileNameWithoutExtension(args[1]) + "_encryptedHeader.dat"), headerData);
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
