using Newtonsoft.Json;
using ProtoGram.Protocol.Encoding.Dynamic;
using ProtoGram.Protocol.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProtoGram.Types
{
    [DataContract]
    public class DynamicMemberValue : IDynamicMemberValue
    {
        [JsonIgnore]
        public DynamicMemberDescription Description { get; set; }

        public object Value { get; set; }

        public byte[] ToByteArray(Dictionary<string, IDynamicValue> _data)
        {
            if (Description.IsArray)
            {
                long length = 0;
                var arrayLength = Description.ArrayLength;
                if (!Int64.TryParse(arrayLength, out length))
                {
                    //is reference value
                    length = Convert.ToInt64(_data[arrayLength].Value);
                }
                List<byte> bytes = new List<byte>();
                if (Description.IsContract)
                {
                    //array of contract
                    for (int i = 0; i < length; i++)
                    {
                        bytes.AddRange(Helper.ToByteArray(Convert.ToByte(((object[])Value)[i]), Description.TypeCode, Description.LengthInBits));
                    }
                }
                else
                {
                    //array of simple type
                    for (int i = 0; i < length; i++)
                    {
                        bytes.AddRange(Helper.ToByteArray(Convert.ToByte(((object[])Value)[i]), Description.TypeCode, Description.LengthInBits));
                    }
                }
                return bytes.ToArray();
            }
            else
            {
                //simple type
                return Helper.ToByteArray(Value, Description.TypeCode, Description.LengthInBits);
            }
        }

      


    }
}
