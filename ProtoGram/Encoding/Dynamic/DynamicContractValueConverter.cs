using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProtoGram.Types
{
    internal class DynamicContractValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(DynamicContractValue) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            // Load JObject from stream
            JObject jObject = JObject.Load(reader);
            var res = DeserializeDynamicContractValue(jObject);
            return res;
        }

        private DynamicContractValue DeserializeDynamicContractValue(JObject obj)
        {
            DynamicContractValue target = new DynamicContractValue();
            foreach (var child in obj.Children())
            {
                if (child.GetType() == typeof(JProperty))
                {
                    if (((JProperty)child).Value.Type == JTokenType.Object)
                        target[((JProperty)child).Name] = DeserializeDynamicContractValue(((JProperty)child).Value as JObject);
                    else if (((JProperty)child).Value.Type == JTokenType.Array)
                    {
                        var jarray = ((JArray)((JProperty)child).Value);
                        var temp = new object[jarray.Count];
                        for (int i = 0; i < jarray.Count; i++)
                        {
                            if (jarray[i] is JObject)
                                temp[i] = DeserializeDynamicContractValue((JObject)jarray[i]);
                            else
                                temp[i] = ((JValue)jarray[i]).Value;
                        }
                        target[((JProperty)child).Name] = new DynamicMemberValue(null, temp);
                    }
                    else
                        target[((JProperty)child).Name] = new DynamicMemberValue(null, ((JValue)((JProperty)child).Value).Value);
                }
            }
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            writer.WriteStartObject();

            foreach (var property in ((DynamicContractValue)value).Data)
            {
                writer.WritePropertyName(property.Key);
                if (property.Value is DynamicMemberValue)
                    serializer.Serialize(writer, ((DynamicMemberValue)property.Value).Value);
                else
                    serializer.Serialize(writer, property.Value);
            }

            writer.WriteEndObject();
        }
    }
}