using System;
using System.Collections.Generic;
using System.Text;

namespace HorizonCrypt
{
    internal sealed class SeadRandom
    {
        public readonly uint[] internalData;

        public SeadRandom(uint seedOne, uint seedTwo, uint seedThree, uint seedFour)
        {
            internalData = new[] { seedOne, seedTwo, seedThree, seedFour };
        }

        public SeadRandom(uint seed)
        {
            internalData = new uint[4];
            for (int i = 0; i < 4; i++)
            {
                internalData[i] = (uint)((0x6C078965 * (seed ^ (seed >> 30))) + i + 1);
                seed = internalData[i];
            }
        }

        public uint GetU32()
        {
            uint v1 = internalData[0] ^ (internalData[0] << 11);

            internalData[0] = internalData[1];
            internalData[1] = internalData[2];
            internalData[2] = internalData[3];
            return internalData[3] = v1 ^ (v1 >> 8) ^ internalData[3] ^ (internalData[3] >> 19);
        }

        public ulong GetU64()
        {
            uint v1 = internalData[0] ^ (internalData[0] << 11);
            uint v2 = internalData[1];
            uint v3 = v1 ^ (v1 >> 8) ^ internalData[3];

            internalData[0] = internalData[2];
            internalData[1] = internalData[3];
            internalData[2] = v3 ^ (internalData[3] >> 19);
            internalData[3] = v2 ^ (v2 << 11) ^ ((v2 ^ (v2 << 11)) >> 8) ^ internalData[2] ^ (v3 >> 19);
            return ((ulong)internalData[2] << 32) | internalData[3];
        }
    }
}
