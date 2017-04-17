using System;

namespace ProtoGram.Protocol.Encoding
{ 

    [AttributeUsage(AttributeTargets.Property)]
    public class BinaryEncodingMemberAttribute : Attribute
    {
        public int Order { get; set; }
        public int Size { get; set; }
        public string Condition { get; set; }
    }
}