using ProtoGram.Types;
using System.Collections.Generic;

namespace ProtoGram.Protocol.Interfaces
{
    public interface IDynamicContractValue : IDynamicValue
    {
        IDynamicValue this[string elementName] { get; set; }
        Dictionary<string, IDynamicValue> Data { get; }
        DynamicContractDescription DynamicMessageDescription { get; set; }
        byte[] ToByteArray(DynamicContractDescription dynamicMessageDescription);
    }
}