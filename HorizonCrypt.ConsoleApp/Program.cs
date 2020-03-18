using System;
using System.IO;

namespace HorizonCrypt.ConsoleApp
{
    internal static class Program
    {
        private const string Encrypt   = "-c";
        private const string Decrypt   = "-d";
        private const string BatchMode = "-b";

        private enum Mode
        { 
            Encrypt, Decrypt
        }

        private static void Main(string[] args)
        {
            if (!TryExecute(args))
                PrintUsage();
            else
                Console.WriteLine("Done!");
            Console.Read();
        }

        private static bool TryExecute(string[] args)
        {
            if (args.Length < 2)
                return false;

            var file = args[args.Length - 1];
            if (!File.Exists(file) && !Directory.Exists(file))
                return false;

            file = Path.GetFullPath(file);
            var directory = Path.GetDirectoryName(file);
            if (directory == null)
                return false;

            var argumentsValid = false;
            var batchMode = false;
            var mode = Mode.Decrypt;
            for (var i = 0; i < args.Length - 1; i++)
            {
                var type = args[i];
                switch (type)
                {
                    case Decrypt:
                        {
                            if (argumentsValid)
                                return false;
                            mode = Mode.Decrypt;
                            argumentsValid = true;
                            break;
                        }

                    case Encrypt:
                        {
                            if (argumentsValid)
                                return false;
                            mode = Mode.Encrypt;
                            argumentsValid = true;
                            break;
                        }

                    case BatchMode:
                        {
                            batchMode = true;
                            break;
                        }

                    default:
                        return false;
                }
            }

            if (!argumentsValid)
                return false;

            // Create out directory to preserve original files.
            var filename = Path.GetFileName(file);
            if (batchMode && string.IsNullOrWhiteSpace(filename))
            {
                file = directory;
                filename = Path.GetFileName(file);
                directory = Path.GetDirectoryName(directory);
            }

            var outDir = Path.Combine(directory, filename + (mode == Mode.Decrypt ? " Decrypted" : " Encrypted"));
            Console.WriteLine("OutDir=" + outDir);

            // In batch mode the file will be the root directory.
            if (batchMode)
            {
                var rootDir = file;
                if (Directory.Exists(rootDir))
                {
                    ProcessFolder(rootDir, outDir, mode);
                    return true;
                }

                return false;
            }

            // Process an individual file.
            if (File.Exists(file))
            {
                ProcessFile(file, directory, outDir, mode);
                return true;
            }

            return false;
        }

        private static void ProcessFolder(in string directory, in string outDir, in Mode mode)
        {
            foreach (var file in Directory.GetFiles(directory, "*.dat"))
                ProcessFile(file, directory, outDir, mode);

            foreach (var subDir in Directory.GetDirectories(directory))
                ProcessFolder(subDir, Path.Combine(outDir, Path.GetFileName(subDir)), mode);
        }

        private static void ProcessFile(in string file, in string directory, in string outDir, in Mode mode)
        {
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);
            var filename = Path.GetFileNameWithoutExtension(file);
            if (!filename.Contains("Header"))
            {
                switch (mode)
                {
                    case Mode.Decrypt:
                        {
                            var headerPath = Path.Combine(directory, filename + "Header.dat");
                            var headerData = File.ReadAllBytes(headerPath);
                            var encData = File.ReadAllBytes(file);

                            var decrypted = Encryption.Decrypt(headerData, encData);
                            File.WriteAllBytes(Path.Combine(outDir, filename + ".dat"), decrypted);
                            Console.WriteLine($"Decrypted: {file}");
                            break;
                        }

                    case Mode.Encrypt:
                        {
                            var decData = File.ReadAllBytes(file);
                            var seed = (uint)DateTime.Now.Ticks;
                            var (data, headerData) = Encryption.Encrypt(decData, seed);

                            File.WriteAllBytes(Path.Combine(outDir, filename + ".dat"), data);
                            File.WriteAllBytes(Path.Combine(outDir, filename + "Header.dat"), headerData);
                            Console.WriteLine($"Encrypted {file}");
                            break;
                        }
                }
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("HorizonCrypt by Cuyler");
            Console.WriteLine("Usage:");
            Console.WriteLine("\tHorizonCrypt [-b] [-c|-d] <input>");
        }
    }
}
