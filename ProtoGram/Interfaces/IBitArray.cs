using System;

namespace ProtoGram.Protocol.Buffer
{
    public interface IBitArray
    {
        bool this[int index] { get; }
        int Length { get; }
        byte[] Pop(int sizeInBits);
        object PopSingleValue(int lenthInBits, TypeCode typeCode);
        void Push(byte[] bytes, int sizeInBits);
        byte[] ToByteArray();
    }
}