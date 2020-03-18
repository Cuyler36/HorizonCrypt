using System;

namespace HorizonCrypt
{
    public static class Encryption
    {
        private static byte[] GetParam(in uint[] data, in int idx)
        {
            var prms = data[data[idx + 1] & 0x7F] & 0x7F;
            var sead = new SeadRandom(data[data[idx] & 0x7F]);
            var rndRollCount = (prms & 0xF) + 1;

            for (var i = 0; i < rndRollCount; i++)
                sead.GetU64();

            var ret = new byte[16];
            for (var i = 0; i < 16; i++)
                ret[i] = (byte)(sead.GetU32() >> 24);

            return ret;
        }

        public static byte[] Decrypt(in byte[] headerData, in byte[] encData)
        {
            // First 256 bytes go unused
            var importantData = new uint[128];
            Buffer.BlockCopy(headerData, 0x100, importantData, 0, 0x200);

            // Set up Key
            var key = GetParam(importantData, 0);

            // Set up counter
            var counter = GetParam(importantData, 2);

            // Do the AES
            using (var aesCtr = new Aes128CounterMode(counter))
            {
                var transform = aesCtr.CreateDecryptor(key, counter);
                var decData = new byte[encData.Length];

                transform.TransformBlock(encData, 0, encData.Length, decData, 0);
                return decData;
            }
        }

        private static uint[] PrependedData =
        {
            0x00000067, 0x0000006F, 0x00000002, 0x00000002, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000
        };

        private static (byte[], byte[], byte[]) GenerateHeaderFile()
        {
            var encryptData = new uint[0x80];

            // Generate 128 Random uints which will be used for params
            var random = new SeadRandom((uint)DateTime.Now.Ticks);
            for (var i = 0; i < 128; i++)
                encryptData[i] = random.GetU32();

            var headerData = new byte[0x300];
            Buffer.BlockCopy(PrependedData, 0, headerData, 0, 0x100);
            Buffer.BlockCopy(encryptData, 0, headerData, 0x100, 0x200);
            return (headerData, GetParam(encryptData, 0), GetParam(encryptData, 2));
        }

        public static (byte[], byte[]) Encrypt(in byte[] data)
        {
            // Generate header file and get key and counter
            var (headerData, key, ctr) = GenerateHeaderFile();

            // Encrypt file
            using (var aesCtr = new Aes128CounterMode(ctr))
            {
                var transform = aesCtr.CreateEncryptor(key, ctr);
                var encData = new byte[data.Length];
                transform.TransformBlock(data, 0, data.Length, encData, 0);

                return (encData, headerData);
            }
        }
    }
}
