using ProtoGram.Protocol.Buffer;
using ProtoGram.Protocol.Interfaces;
using ProtoGram.Types;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ProtoGram.Protocol.Encoding.Dynamic
{
    public static class Helper
    {
        
      
        public static bool TryParseContract(DynamicContractDescription contract,
            Buffer.BitArray buffer, out DynamicContractValue msg)
        {
            msg = new DynamicContractValue(contract);
            foreach (var property in contract.Members)
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
                            DynamicContractValue value;

                            if (!TryParseContract(property.Value.ContractDescription, buffer, out value))
                                return false;
                            temp[i] = value;
                        }
                        propValue.Value = temp;
                        msg.Data[property.Value.Name] = propValue;
                    }
                    else
                    {
                        DynamicContractValue value;

                        if (!TryParseContract(property.Value.ContractDescription, buffer, out value))
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
        
        public static byte[] ToByteArray(object value, TypeCode typeCode, int lenght)
        {
            byte[] temp = null;
            switch (typeCode)
            {
                case TypeCode.Int32:
                    temp = Protocol.Buffer.BitConverter.FromInt32(Convert.ToInt32(value), lenght);
                    break;
                case TypeCode.Int64:
                    temp = Protocol.Buffer.BitConverter.FromInt64(Convert.ToInt64(value), lenght);
                    break;
                case TypeCode.Byte:
                    temp = Protocol.Buffer.BitConverter.FromByte(Convert.ToByte(value), lenght);
                    break;
                case TypeCode.Char:
                    temp = Protocol.Buffer.BitConverter.FromChar(Convert.ToChar(value), lenght);
                    break;
                case TypeCode.Boolean:
                    temp = Protocol.Buffer.BitConverter.FromBoolean(Convert.ToBoolean(value), lenght);
                    break;
                default: throw new NotImplementedException();
            }

            return temp;

        }
    }
}
