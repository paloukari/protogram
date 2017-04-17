﻿using System;

namespace ProtoGram.Protocol.Atp.Encoding
{
    [AttributeUsage(AttributeTargets.Property)]
    public class BinaryEncodingListAttribute : BinaryEncodingMemberAttribute
    {
        public string ArraySize { get; set; }
    }
}