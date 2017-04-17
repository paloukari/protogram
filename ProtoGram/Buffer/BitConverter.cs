using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtoGram.Protocol.Buffer
{
    public static class BitConverter
    {
        public static int ToInt32(byte[] data)
        {
            return (int)ToInt64(data);
        }

        public static long ToInt64(byte[] data)
        {
            int pos = (data.Length - 1) * 8;
            long result = 0;
            for (int i = 0; i < data.Length; i++)
            {
                result |= (long)(data[i] << pos);
                pos -= 8;
            }
            return result;
        }
        public static object ToBoolean(byte[] bytes)
        {
            return bytes[0] != 0;
        }

        public static byte ToByte(byte[] bytes)
        {
            return bytes[0];
        }
        public static char ToChar(byte[] bytes)
        {
            return (char)bytes[0];
        }

        public static byte[] FromInt32(int value, int sizeInBits)
        {
            return FromInt64(value, sizeInBits);
        }

        public static byte[] FromInt64(long value, long sizeInBits)
        {
            var res = System.BitConverter.GetBytes(value);
            var bytes = (int)Math.Ceiling(sizeInBits / 8.0);
            if (res.Length != bytes)
            {
                var res2 = new byte[bytes];
                Array.Copy(res, res2, bytes);
                Array.Reverse(res2);
                res = res2;
            }
            return res;
        }

        public static byte[] FromBoolean(bool value, long sizeInBits = 1)
        {
            return new byte[] { value ? (byte)1 : (byte)0 };
        }

        public static byte[] FromByte(byte value, int sizeInBits)
        {
            return new byte[] { value };
        }
        public static byte[] FromChar(char value, int sizeInBits)
        {
            return new byte[] { (byte)value };
        }
    }
}
