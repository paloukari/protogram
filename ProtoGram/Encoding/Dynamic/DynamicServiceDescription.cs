using ProtoGram.Protocol.Interfaces;
using System.Collections.Generic;

namespace ProtoGram.Types
{
    public class DynamicServiceDescription 
    {
        protected Dictionary<int, DynamicContractDescription> _dynamicMessageDescriptions;

        public virtual int ServiceId { get; set; }

        public Dictionary<int, DynamicContractDescription> DynamicMessageDescriptions
        {
            get
            {
                return _dynamicMessageDescriptions;
            }

            set
            {
                _dynamicMessageDescriptions = value;
            }
        }

        public DynamicServiceDescription()
        {
            _dynamicMessageDescriptions = new Dictionary<int, DynamicContractDescription>();
        }

        public void AddProperty(DynamicContractDescription description)
        {
            _dynamicMessageDescriptions[description.Type] = description;
        }

        public DynamicContractDescription GetDynamicContractDescription(int typeId)
        {
            return _dynamicMessageDescriptions[typeId];
        }
    }
}
