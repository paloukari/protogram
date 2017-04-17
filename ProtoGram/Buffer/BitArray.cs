using ProtoGram.Protocol.Buffer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtoGram.Protocol.Buffer
{
    public class BitArray : IBitArray
    {
        bool[] _data;
        public BitArray(byte[] data)
        {
            System.Collections.BitArray temp = new System.Collections.BitArray(data);
            _data = new bool[temp.Length];
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                    _data[8 * i + j] = (bool)temp[8 * i + (7 - j)];
            }

        }

        public BitArray()
        {
            _data = new bool[0];
        }

        public void Push(byte[] bytes, int sizeInBits)
        {
            var newData = new bool[_data.Length + sizeInBits];
            BitArray addition = new BitArray(bytes);

            if (_data.Length > 0)
                Array.Copy(_data, 0, newData, 0, _data.Length);
            if (sizeInBits < 8)
                Array.Copy(addition._data, addition.Length - sizeInBits, newData, _data.Length, sizeInBits);
            else
                Array.Copy(addition._data, 0, newData, _data.Length, sizeInBits);

            _data = newData;
        }
        public byte[] Pop(int sizeInBits)
        {
            if (sizeInBits == Const.READTOEND)
                sizeInBits = _data.Length;
            List<byte> result = new List<byte>();
            var sizeInBitsTemp = sizeInBits;
            while (sizeInBitsTemp > 0)
            {
                if (sizeInBitsTemp / 8 > 0)
                {
                    result.Add(PopBits(8));
                    sizeInBitsTemp = sizeInBitsTemp - 8;
                }
                else
                {
                    byte byt = PopBits(sizeInBitsTemp);
                    result.Add(byt);
                    sizeInBitsTemp = 0;
                }
            }
            return result.ToArray();
        }
        public int Length { get { return _data.Length; } }
        public bool this[int index]
        {
            get { return _data[index]; }
        }
        public byte[] ToByteArray()
        {
            byte[] data = new byte[(int)Math.Ceiling(_data.Length / 8.0)];

            var copy = DeepCopy();
            int remainingBits = copy._data.Length;
            int i = 0;
            while (remainingBits > 0)
            {
                if (remainingBits >= 8)
                {
                    data[i++] = copy.PopBits(8);
                    remainingBits -= 8;
                }
                else
                {
                    var lastByte= copy.PopBits(remainingBits);
                    lastByte = (byte)(lastByte << (8 - remainingBits));
                    data[i++] = lastByte;
                    remainingBits = 0;
                }
            }
            return data;
        }
        public object PopSingleValue(int lenthInBits, TypeCode typeCode)
        {
            object temp;
            var bytes = Pop(lenthInBits);
            switch (typeCode)
            {
                case TypeCode.Int32:
                    temp = Buffer.BitConverter.ToInt32(bytes);
                    break;
                case TypeCode.Int64:
                    temp = Buffer.BitConverter.ToInt64(bytes);
                    break;
                case TypeCode.Byte:
                    temp = Buffer.BitConverter.ToByte(bytes);
                    break;
                case TypeCode.Boolean:
                    temp = Buffer.BitConverter.ToBoolean(bytes);
                    break;
                case TypeCode.Char:
                    temp = Buffer.BitConverter.ToChar(bytes);
                    break;
                default: throw new NotImplementedException();
            }

            return temp;
        }
        private byte PopBits(int bits)
        {
            byte byt = 0;
            for (int i = 0; i < bits; i++)
                byt = (byte)((byt << 1) | (this[i] ? 1 : 0));
            InternalPop(bits);
            return byt;
        }
        private void InternalPop(int sizeInBits)
        {
            bool[] tempData = new bool[_data.Length - sizeInBits];
            Array.Copy(_data, sizeInBits, tempData, 0, tempData.Length);
            _data = tempData;
        }
        private BitArray DeepCopy()
        {
            var copy = new BitArray();
            copy._data = new bool[_data.Length];
            Array.Copy(_data, copy._data, _data.Length);
            return copy;
        }
    }
}
