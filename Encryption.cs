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
            using var aesCtr = new Aes128CounterMode(counter);
            var transform = aesCtr.CreateDecryptor(key, counter);
            var decData = new byte[encData.Length];

            transform.TransformBlock(encData, 0, encData.Length, decData, 0);
            return decData;
        }
    }
}
