
namespace ProtoGram.Protocol.Interfaces
{
    using Newtonsoft.Json;
    public abstract class Dynamic
    {
        protected IDynamicContractValue _value;
        public Dynamic(IDynamicContractValue value)
        {
            _value = value;
        }
        [JsonIgnore]
        public IDynamicContractValue Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }
    }
}
