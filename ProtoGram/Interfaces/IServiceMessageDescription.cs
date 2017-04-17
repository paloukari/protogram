using ProtoGram.Types;

namespace ProtoGram.Protocol.Interfaces
{
    public interface IServiceMessageDescription
    {
        DynamicContractDescription Description { get; }
    }
}
