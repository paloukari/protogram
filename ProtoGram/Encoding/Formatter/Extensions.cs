using ProtoGram.Protocol.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProtoGram.Protocol.Encoding.Formatter
{
    public static class Extensions
    {
        public static bool HasAttribute(this Type type, Type attribute)
        {
            return type.CustomAttributes.SingleOrDefault(e => e.AttributeType == attribute) != null;
        }
        public static CustomAttributeData GetAttributeFamily(this PropertyInfo property, Type attribute)
        {
            return property.CustomAttributes.SingleOrDefault(e => e.AttributeType.IsSubclassOf(attribute) || e.AttributeType == attribute);
        }
        public static bool HasAttributeFamily(this PropertyInfo property, Type attribute)
        {
            return property.GetAttributeFamily(attribute) != null;
        }
        public static CustomAttributeNamedArgument? GetAttributeArgumentValue(this PropertyInfo property, Type attribute, string propertyName)
        {
            var attributeData = property.GetAttributeFamily(attribute);
            if (attributeData == null)
                return null;
            return attributeData.NamedArguments.SingleOrDefault(e => e.MemberName == propertyName);
        }
        public static int? GetBitSerializationAttributeIntValue(this PropertyInfo property, string propertyName)
        {
            var res = property.GetAttributeArgumentValue(typeof(BinaryEncodingMemberAttribute), propertyName);
            if (!res.HasValue)
                return null;
            if (res.Value.TypedValue == null)
                return null;
            if (res.Value.TypedValue.Value != null)
                return (int)res.Value.TypedValue.Value;
            else return null;
        }
        public static string GetBitSerializationAttributeStringValue(this PropertyInfo property, string propertyName)
        {
            var res = property.GetAttributeArgumentValue(typeof(BinaryEncodingMemberAttribute), propertyName);
            if (!res.HasValue)
                return null;
            return res.Value.TypedValue.Value as string;
        }
        public static bool IsSerializable(this PropertyInfo property)
        {
            return property.HasAttributeFamily(typeof(BinaryEncodingMemberAttribute));
        }


        public static bool GetPropertyConditionResult(this PropertyInfo property, object graph)
        {
            var condition = property.GetBitSerializationAttributeStringValue("Condition");
            if (condition != null)
            {
                var prop = graph.GetType().GetProperty(condition);
                if (prop != null)
                    if (prop.GetValue(graph) is bool)
                        return (bool)prop.GetValue(graph);
            }

            return true;
        }
        public static int GetPropertyBitsLength(this PropertyInfo property, object graph)
        {
            if (!GetPropertyConditionResult(property, graph))
                return 0;
            var size = property.GetBitSerializationAttributeIntValue("Size");
            if (size.HasValue)
                return size.Value;
            return GetContractBitsCount(property.PropertyType, graph);
        }
        public static int GetPropertyArraySize(this PropertyInfo property, object graph)
        {
            var size = property.GetBitSerializationAttributeStringValue("ArraySize");
            if (size != null)
            {
                int res = 0;
                if (Int32.TryParse(size as string, out res))
                    return res;
                //it's a reference property of the object
                var prop = graph.GetType().GetProperty(size);
                if (prop != null)
                    if (prop.GetValue(graph) is int)
                        return (int)prop.GetValue(graph);
            }
            return -1;
        }
        public static int GetContractBitsCount(this Type type, object graph)
        {
            var props = GetOrderedSerializableProperties(type);

            int res = 0;
            foreach (var prop in props)
                res += GetPropertyBitsLength(prop, graph);
            return res;
        }
        public static PropertyInfo[] GetOrderedSerializableProperties(this Type type)
        {
            var properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (properties != null)
            {
                var serializableOrderedProperties = properties.Where(e => e.IsSerializable()).OrderBy(e => e.GetBitSerializationAttributeIntValue("Order"));
                return serializableOrderedProperties.ToArray();
            }
            return null;
        }
    }
}
