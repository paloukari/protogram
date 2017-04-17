using ProtoGram.Types;
using System.Collections.Generic;

namespace ProtoGram.Protocol.Interfaces
{
    public interface IDynamicMemberValue: IDynamicValue
    {
        DynamicMemberDescription Description { get; set; }

        byte[] ToByteArray(Dictionary<string, IDynamicValue> _data);
    }
}