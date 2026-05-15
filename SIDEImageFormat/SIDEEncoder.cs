using System;
using System.Collections.Generic;
using System.Text;

namespace SIDEImageFormat
{
    internal class SIDEEncoder
    {

        private struct ColorWithOccurrences
        {
            public uint Color { get; set; }
            public uint Occurrences { get; set; }

            public ColorWithOccurrences(uint color, uint occurrences)
            {
                Color = color;
                Occurrences = occurrences;
            }
        }

        public static byte[] ToSIDE(uint width, uint height, uint[] image)
        {
            List<byte> data = new List<byte>(19);
            data.AddRange([0x53, 0x49, 0x44, 0x45, 0x49, 0x4D, 0x47]);
            data.AddRange(UintToBytes(width));
            data.AddRange(UintToBytes(height));
            Dictionary<uint, uint> ColorOccurrences = new Dictionary<uint, uint>();
            foreach(uint i in image)
            {
                if (ColorOccurrences.ContainsKey(i)) ColorOccurrences[i]++;
                else ColorOccurrences.Add(i, 1);
            }
            List<ColorWithOccurrences> l = new List<ColorWithOccurrences>();
            foreach (uint k in ColorOccurrences.Keys) l.Add(new ColorWithOccurrences(k, ColorOccurrences[k]));
            l.Sort((a, b) => b.Occurrences.CompareTo(a.Occurrences));
            data.AddRange(UintToBytes((uint) l.Count));
            foreach (ColorWithOccurrences c in l) data.AddRange(UintToBytes(c.Color));
            Dictionary<uint, uint> ColorIndices = new Dictionary<uint, uint>();
            for(uint i = 0; i < l.Count; i++) ColorIndices.Add(l[(int) i].Color, i + 1);
            uint lastIdx = 0;
            uint count = 0;
            foreach(uint i in image)
            {
                uint idx = ColorIndices[i];
                if (idx != lastIdx)
                {
                    WriteRLE(lastIdx, count, data);
                    lastIdx = idx;
                    count = 1;
                }
                else count++;
            }
            if (count > 0) WriteRLE(lastIdx, count, data);
            return data.ToArray();
        }

        private static void WriteRLE(uint idx, uint count, List<byte> data)
        {
            uint rleByteCount = 1 + (uint)VarIntLength(count) + (uint)VarIntLength(idx);
            uint rawByteCount = (uint)VarIntLength(idx) * count;
            if (rleByteCount < rawByteCount)
            {
                data.Add(0x00);
                data.AddRange(EncodeVarInt(count));
                data.AddRange(EncodeVarInt(idx));
            }
            else for (uint j = 0; j < count; j++) data.AddRange(EncodeVarInt(idx));
        }

        private static int VarIntLength(uint i)
        {
            int l = 0;
            do
            {
                l++;
                i >>= 7;
            } while (i != 0);
            return l;
        }

        private static byte[] EncodeVarInt(uint i)
        {
            byte[] b = new byte[VarIntLength(i)];
            for(int j = 0; j < b.Length; j++)
            {
                byte data = (byte) (i & 0b01111111);
                i >>= 7;
                if (j != b.Length - 1) data |= 0b10000000;
                b[j] = data;
            }
            return b;
        }

        private static byte[] UintToBytes(uint i)
        {
            return [(byte) ((i >> 24) & 0xFF), (byte)((i >> 16) & 0xFF), (byte)((i >> 8) & 0xFF), (byte)(i & 0xFF)];
        }

    }
}
