using ProtoGram.Protocol.Interfaces;
using ProtoGram.Protocol.Types;
using ProtoGram.Types;
using System.Collections.Generic;

namespace ProtoGram.Types
{
    public class DynamicMemberCondition 
    {
        public CompareOperator CompareOperator { get; set; }
        public string PropertyName { get; set; }
        public string Value { get; set; }

        public DynamicMemberCondition()
        {

        }
        public static DynamicMemberCondition CreateDynamicMemberCondition(string propertyName, CompareOperator compareOperator, string value)
        {
            return new DynamicMemberCondition()
            {
                PropertyName = propertyName,
                CompareOperator = compareOperator,
                Value = value,
            };
        }
        public bool Evaluate(Dictionary<string, IDynamicValue> _data)
        {
            if (_data.ContainsKey(PropertyName))
            {
                var value = _data[PropertyName].Value.ToString();
                switch (CompareOperator)
                {
                    case CompareOperator.Equal:
                        return value == Value;
                    default:
                        break;
                }
            }
            return false;
        }
    }
}