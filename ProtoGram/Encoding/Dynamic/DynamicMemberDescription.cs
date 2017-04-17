using ProtoGram.Protocol.Interfaces;
using System;

namespace ProtoGram.Types
{
    public class  DynamicMemberDescription 
    {
        private DynamicContractDescription _contractDescription;
        private DynamicMemberCondition _condition;

        public DynamicMemberDescription()
        {

        }
        public static DynamicMemberDescription CreateDynamicMemberDescription(string name,
            int lengthInBits,
            TypeCode typeCode,
            object defaultValue,
            string arrayLength = null,
            DynamicMemberCondition condition = null,
            DynamicContractDescription contractDescription = null)
        {
            return new DynamicMemberDescription()
            {
                Name = name,
                LengthInBits = lengthInBits,
                ArrayLength = arrayLength,
                TypeCode = typeCode,
                Default = defaultValue,
                Condition = condition,
                ContractDescription = contractDescription
            };
        }

        public static DynamicMemberDescription CreateDynamicMemberDescription(string name,
           DynamicContractDescription contractDescription)
        {
            return new DynamicMemberDescription()
            {
                Name = name,
                ContractDescription = contractDescription,
                Condition = null
            };
        } 

        public static DynamicMemberDescription CreateDynamicMemberDescription(string name,
           DynamicContractDescription contractDescription, string arrayLength,
            DynamicMemberCondition condition = null)
        {
            return new DynamicMemberDescription()
            {
                Name = name,
                ContractDescription = contractDescription,
                ArrayLength = arrayLength,
                Condition = condition
            };
        }


        public static DynamicMemberDescription CreateDynamicMemberDescription(string name,
           DynamicContractDescription contractDescription,
           DynamicMemberCondition condition = null)
        {

            return new DynamicMemberDescription()
            {
                Name = name,
                ContractDescription = contractDescription,
                Condition = condition,
            };
        }

        public static DynamicMemberDescription CreateDynamicMemberDescription(string name,
            DynamicContractDescription contractDescription,
            string arrayLength)
        {

            return new DynamicMemberDescription()
            {
                Name = name,
                ContractDescription = contractDescription,
                ArrayLength = arrayLength
            };
        }


        public string Name { get; set; }
        public TypeCode TypeCode { get; set; }
        public int LengthInBits { get; set; }
        public object Default { get; set; }
        public bool IsArray { get { return !string.IsNullOrEmpty(ArrayLength); }}
        public bool IsContract { get { return _contractDescription != null; } }
        public bool IsConditional { get { return _condition != null; } }
        public string ArrayLength { get; set; }

        public DynamicContractDescription ContractDescription
        {
            get
            {
                return _contractDescription;
            }

            set
            {
                _contractDescription = value;
            }
        }

        public DynamicMemberCondition Condition
        {
            get
            {
                return _condition;
            }

            set
            {
                _condition = value;
            }
        }

        public bool TryParseDynamicPropertyValue(Protocol.Buffer.IBitArray buffer,
           IDynamicContractValue msg,
           out IDynamicMemberValue value)
        {
            value = null;

            if (buffer.Length < this.LengthInBits)
                return false;

            var res = new DynamicMemberValue();

            res.Description = this;

            if (this.IsArray)
            {
                long length = 0;
                var arrayLength = this.ArrayLength;

                if (!Int64.TryParse(arrayLength, out length))
                    length = Convert.ToInt32(msg.Data[arrayLength].Value);

                object[] array = new object[length];

                if (buffer.Length < this.LengthInBits * length)
                    return false;

                for (int i = 0; i < length; i++)
                {
                    array[i] = buffer.PopSingleValue(res.Description.LengthInBits, this.TypeCode);
                }
                var tmp = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    tmp[i] = Convert.ToByte(array[i]);
                }
                res.Value = array;
            }
            else
            {
                res.Value = buffer.PopSingleValue(res.Description.LengthInBits, this.TypeCode);
            }
            value = res;
            return true;
        }

        public bool TryParseDynamicPropertyValue(byte[] array, IDynamicContractValue msg, out IDynamicMemberValue value)
        {
            Protocol.Buffer.BitArray buffer = new Protocol.Buffer.BitArray(array);
            return TryParseDynamicPropertyValue(buffer, msg, out value);
        }


    }
}