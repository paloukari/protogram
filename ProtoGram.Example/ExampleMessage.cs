using ProtoGram.Protocol;
using ProtoGram.Protocol.Encoding;
using ProtoGram.Protocol.Interfaces;
using ProtoGram.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtoGram.Example
{
    [BinaryEncodingContract]
    public class ExampleMessage
    {
        [BinaryEncodingMember(Order = 0)]
        public MessageHeader MessageHeader { get; set; }


        [BinaryEncodingMember(Order = 1)]
        public MessageDynamic MessageDynamic { get; set; }
    }

    [BinaryEncodingContract]
    public class MessageDynamic
    {
        private IDynamicContractValue _dynamicMessage;

        [BinaryEncodingMember(Order = 0, Size = Const.READTOEND)]
        public byte[] Data { get; set; }

        public IDynamicContractValue DynamicMessage { get { return _dynamicMessage; } set { _dynamicMessage = value; } }

        internal void Parse(DynamicContractDescription dynamicMessageDescription)
        {
            if (Data != null)
            {
                if (!dynamicMessageDescription.TryParseContract(Data, out _dynamicMessage))
                    _dynamicMessage = null;
            }
            else if (_dynamicMessage != null)
                Data = _dynamicMessage.ToByteArray(dynamicMessageDescription);
        }
    }

    [BinaryEncodingContract]
    public class MessageHeader
    {
        [BinaryEncodingMember(Order = 0, Size = 3 * 8)]
        public int Id { get; set; }

        [BinaryEncodingMember(Order = 2, Size = 4 * 8, Condition = "IdCondition")]
        public long ConditionalId { get; set; }

        [BinaryEncodingMember(Order = 3, Size = 1 * 8)]
        public int ArrayLength { get; set; }

        [BinaryEncodingList(Order = 4, ArraySize = "ArrayLength")]
        public SimpleByte[] Digest { get; set; }

        public bool IdCondition
        {
            get
            {
                if (Id > 100)
                    return true;
                return false;
            }
        }
    }

    [BinaryEncodingContract]
    public class SimpleByte
    {
     
        [BinaryEncodingMember(Order = 0, Size = 1 * 8)]
        public byte Data { get; set; }

    }
}
