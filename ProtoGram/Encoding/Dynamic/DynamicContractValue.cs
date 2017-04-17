using Newtonsoft.Json;
using ProtoGram.Protocol.Interfaces;
using ProtoGram.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProtoGram.Types
{
    [DataContract]
    [JsonConverter(typeof(DynamicContractValueConverter))]
    public class DynamicContractValue : IDynamicContractValue
    {
        private DynamicContractDescription _dynamicMessageDescription;
        Dictionary<string, IDynamicValue> _data = new Dictionary<string, IDynamicValue>();

        public object Value { get { return _data; } set { _data = value as Dictionary<string, IDynamicValue>; } }

        public DynamicContractValue()
        {
        }


        //todo:this should be deep copy
        public DynamicContractValue(DynamicContractValue from)
        {
            _dynamicMessageDescription = from._dynamicMessageDescription;
            _data = from._data;
        }

        public DynamicContractValue(DynamicContractDescription dynamicMessageDescription)
        {
            _dynamicMessageDescription = dynamicMessageDescription;
            CreateDefaultValues();
        }

        public DynamicContractValue(Dictionary<string, IDynamicValue> data)
        {
            _data = data;
        }

       
        private void CreateDefaultValues()
        {
            foreach (var item in _dynamicMessageDescription.Members)
                _data.Add(item.Key, new DynamicMemberValue() { Description = item.Value, Value = item.Value.Default });
        }

        public Dictionary<string, IDynamicValue> Data
        {
            get
            {
                return _data;
            }
        }

        public DynamicContractDescription DynamicMessageDescription
        {
            get
            {
                return _dynamicMessageDescription;
            }

            set
            {
                _dynamicMessageDescription = value;
            }
        }

        public IDynamicValue this[string elementName]
        {
            get
            {
                if (_data.ContainsKey(elementName))
                    return _data[elementName];
                return null;
            }

            set
            {
                if (value is DynamicContractValue)
                    _data[elementName] = (DynamicContractValue)value;
                else
                {
                    if (!_data.ContainsKey(elementName))
                        _data[elementName] = new DynamicMemberValue();
                    _data[elementName].Value = value;
                }


            }
        }

        public byte[] ToByteArray(DynamicContractDescription dynamicMessageDescription)
        {
            DynamicMessageDescription = dynamicMessageDescription;

            Protocol.Buffer.BitArray buffer = new Protocol.Buffer.BitArray();
            PushContractData(Data, DynamicMessageDescription, buffer);
            return buffer.ToByteArray();
        }

        public void PushContractData(Dictionary<string, IDynamicValue> data,
           DynamicContractDescription contractDescription,
           Protocol.Buffer.BitArray buffer)
        {
            foreach (var memberDescription in contractDescription.Members)
            {
                if (memberDescription.Value.IsConditional)
                    if (!memberDescription.Value.Condition.Evaluate(data))
                        continue;
                if (memberDescription.Value.IsContract)
                {
                    if (memberDescription.Value.IsArray)
                    {
                        var value = data[memberDescription.Value.Name] as IDynamicMemberValue;

                        var arrayValue = value.Value as System.Collections.IEnumerable;
                        int i = 0;
                        foreach (var item in arrayValue)
                        {
                            i++;
                            //array of contract
                            PushContractData(((DynamicContractValue)item).Data, memberDescription.Value.ContractDescription, buffer);
                        }
                    }
                    else
                        //contract
                        PushContractData(((DynamicContractValue)data[memberDescription.Value.Name]).Data, memberDescription.Value.ContractDescription, buffer);
                }
                else
                {
                    var value = data[memberDescription.Value.Name] as IDynamicMemberValue;

                    value.Description = memberDescription.Value;
                    var val = value.ToByteArray(data);
                    if (memberDescription.Value.IsArray)
                    {
                        int length = 0;
                        var arrayLength = value.Description.ArrayLength;
                        if (!Int32.TryParse(arrayLength, out length))
                        {
                            //is reference value
                            length = Convert.ToInt32(data[arrayLength].Value);
                        }

                        //array of simple type
                        buffer.Push(val, memberDescription.Value.LengthInBits * length);
                    }
                    else
                        //simple type
                        buffer.Push(val, memberDescription.Value.LengthInBits);
                }

            }
        }

    }
}
