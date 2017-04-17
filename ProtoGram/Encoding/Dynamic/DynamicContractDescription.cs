using ProtoGram.Protocol.Encoding.Dynamic;
using ProtoGram.Protocol.Interfaces;
using ProtoGram.Types;
using System;
using System.Collections.Generic;

namespace ProtoGram.Types
{    
    public class DynamicContractDescription 
    {
        public DynamicContractDescription()
        {
            Members = new Dictionary<string, DynamicMemberDescription>();
            
        }
        public virtual string Name { get; set; }
        public Dictionary<string, DynamicMemberDescription> Members { get; set; }
        public virtual int Type { get; set; }

        public bool TryParseContract(byte[] rawData, out IDynamicContractValue msg)
        {
            Protocol.Buffer.BitArray buffer = new Protocol.Buffer.BitArray(rawData);
            return TryParseContract(buffer, out msg);
        }
        public bool TryParseContract(Protocol.Buffer.IBitArray buffer, out IDynamicContractValue msg)
        {
            msg = new DynamicContractValue(this);
            foreach (var property in this.Members)
            {
                if (property.Value.IsConditional)
                    if (!property.Value.Condition.Evaluate(msg.Data))
                    {
                        if (msg.Data.ContainsKey(property.Value.Name))
                            msg.Data.Remove(property.Value.Name);
                        continue;
                    }

                if (property.Value.IsContract)
                {
                    if (property.Value.IsArray)
                    {
                        int length = 0;
                        var arrayLength = property.Value.ArrayLength;
                        if (!Int32.TryParse(arrayLength, out length))
                        {
                            //is reference value
                            length = (int)msg.Data[arrayLength].Value;
                        }
                        object[] temp = new object[length];

                        IDynamicMemberValue propValue = new DynamicMemberValue() { Description = property.Value };

                        for (int i = 0; i < length; i++)
                        {
                            IDynamicContractValue value;

                            if (!property.Value.ContractDescription.TryParseContract(buffer, out value))
                                return false;
                            temp[i] = value;
                        }
                        propValue.Value = temp;
                        msg.Data[property.Value.Name] = propValue;
                    }
                    else
                    {
                        IDynamicContractValue value;

                        if (!property.Value.ContractDescription.TryParseContract(buffer, out value))
                            return false;

                        msg.Data[property.Value.Name] = value;
                    }
                }
                else
                {
                    IDynamicMemberValue value;
                    if (!property.Value.TryParseDynamicPropertyValue(buffer, msg, out value))
                        return false;
                    msg.Data[value.Description.Name] = value;
                }
            }
            return true;
        }

        public DynamicContractDescription GetNestedDynamicContractDescription(string contractName)
        {
            if (Members.ContainsKey(contractName))
                return Members[contractName].ContractDescription;
            foreach (var member in Members)
            {
                if (member.Value.ContractDescription == null)
                    continue;
                var contract = member.Value.ContractDescription.GetNestedDynamicContractDescription(contractName);
                if (contract != null)
                    return contract;
            }
            return null;
        }
    }
}