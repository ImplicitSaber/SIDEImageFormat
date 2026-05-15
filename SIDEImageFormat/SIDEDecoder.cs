using System;
using System.Collections.Generic;
using System.Text;

namespace SIDEImageFormat
{
    internal class SIDEDecoder
    {

        public static uint[] FromSIDE(byte[] data, out uint width, out uint height, out string? error)
        {
            if(data.Length < 19)
            {
                width = 0;
                height = 0;
                error = "header too short";
                return Array.Empty<uint>();
            }
            if (data[0] != 0x53 || data[1] != 0x49 || data[2] != 0x44 || data[3] != 0x45 || data[4] != 0x49 || data[5] != 0x4D || data[6] != 0x47)
            {
                width = 0;
                height = 0;
                error = "invalid header";
                return Array.Empty<uint>();
            }
            width = BytesToUint([data[7], data[8], data[9], data[10]]);
            height = BytesToUint([data[11], data[12], data[13], data[14]]);
            uint paletteSize = BytesToUint([data[15], data[16], data[17], data[18]]);
            uint readerIdx = 19;
            uint[] palette = new uint[paletteSize];
            for(uint i = 0; i < paletteSize; i++)
            {
                if(!CanRead(data, readerIdx, 4))
                {
                    error = "palette data too short";
                    return Array.Empty<uint>();
                }
                palette[i] = BytesToUint(Read(data, ref readerIdx, 4));
            }
            uint[] raw = new uint[width * height];
            for(uint i = 0; i < raw.Length; i++)
            {
                uint idx = ReadVarInt(data, ref readerIdx, out string? varIntErr);
                if(varIntErr != null)
                {
                    error = varIntErr;
                    return Array.Empty<uint>();
                }
                if (idx == 0)
                {
                    uint count = ReadVarInt(data, ref readerIdx, out string? varIntErr2);
                    if (varIntErr2 != null)
                    {
                        error = varIntErr2;
                        return Array.Empty<uint>();
                    }
                    uint rleIdx = ReadVarInt(data, ref readerIdx, out string? varIntErr3);
                    if (varIntErr3 != null)
                    {
                        error = varIntErr3;
                        return Array.Empty<uint>();
                    }
                    for (uint j = 0; j < count; j++)
                    {
                        if (i + j >= raw.Length)
                        {
                            error = "invalid compression";
                            return Array.Empty<uint>();
                        }
                        raw[i + j] = palette[rleIdx - 1];
                    }
                    i += count - 1;
                }
                else raw[i] = palette[idx - 1];
            }
            error = null;
            return raw;
        }

        private static uint ReadVarInt(byte[] bytes, ref uint readerIdx, out string? error)
        {
            uint i = 0;
            for(int count = 0; count < 5; count++)
            {
                if(readerIdx >= bytes.Length)
                {
                    error = "data too short";
                    return 0;
                }
                uint data = ((uint) bytes[readerIdx] & 0b01111111) << (count * 7);
                i |= data;
                if ((bytes[readerIdx] & 0b10000000) == 0)
                {
                    readerIdx++;
                    break;
                }
                readerIdx++;
                if (count == 4)
                {
                    error = "corrupted data";
                    return 0;
                }
            }
            error = null;
            return i;
        }

        private static byte[] Read(byte[] bytes, ref uint readerIdx, uint count)
        {
            byte[] b = new byte[count];
            for (uint i = readerIdx; i < readerIdx + count; i++) b[i - readerIdx] = bytes[i];
            readerIdx += count;
            return b;
        }

        private static bool CanRead(byte[] bytes, uint readerIdx, uint count)
        {
            return readerIdx + count <= bytes.Length;
        }

        private static uint BytesToUint(byte[] bytes)
        {
            return (((uint) bytes[0]) << 24) | (((uint) bytes[1]) << 16) | (((uint) bytes[2]) << 8) | bytes[3];
        }

    }
}
