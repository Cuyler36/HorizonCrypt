using System;
using System.IO;

namespace HorizonCrypt
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args?.Length < 1)
            {
                Console.WriteLine("Enter the path to the encrypted save file:");
                args = new string[] { Console.ReadLine().Replace("\"", "") };
            }

            var directory = Path.GetDirectoryName(args[0]);
            var header = Path.Combine(directory, Path.GetFileNameWithoutExtension(args[0]) + "Header.dat");
            var file = args[0];
            var data = Encryption.Decrypt(File.ReadAllBytes(header), File.ReadAllBytes(file));
            File.WriteAllBytes(Path.Combine(directory, Path.GetFileNameWithoutExtension(args[0]) + "_decrypted.dat"), data);
        }
    }
}
