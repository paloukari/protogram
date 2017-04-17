namespace ProtoGram.Protocol.Interfaces
{
    public abstract class Contract : Dynamic
    {
        public Contract(IDynamicContractValue value) : base(value)
        { }
    }
}
