using Newtonsoft.Json;
using ProtoGram.Protocol.Encoding.Formatter;
using ProtoGram.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtoGram.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var dynamicDescription = JsonConvert.DeserializeObject<DynamicContractDescription>(File.ReadAllText("definition.json"));

            ExampleMessage example = GenerateExample();
            ExampleMessage test = null;

            byte[] exampleData = null;

            using (MemoryStream ms = new MemoryStream())
            {
                var formatter = new BitEncondingFormatter<ExampleMessage>();
                formatter.Serialize(ms, example);
                exampleData = ms.ToArray();
            }

            Console.WriteLine(Convert.ToBase64String(exampleData));


            using (MemoryStream ms = new MemoryStream(exampleData))
            {
                var formatter = new BitEncondingFormatter<ExampleMessage>();
                test = formatter.Deserialize(ms) as ExampleMessage;
            }
            test.MessageDynamic.Parse(dynamicDescription);


            var messageReport = new string(Array.ConvertAll(test.MessageDynamic.DynamicMessage["MessageReport"].Value as object[], (e) => Convert.ToChar(e)));
            var messageStatusCode = (int)test.MessageDynamic.DynamicMessage["MessageStatusCode"].Value;

            Console.WriteLine($"MessageStatusCode = {messageStatusCode}");
            Console.WriteLine($"MessageReport = {messageReport}");

        }

        static ExampleMessage GenerateExample()
        {
            return new ExampleMessage()
            {
                MessageHeader = new MessageHeader()
                {
                    Id = 99,
                    ArrayLength = 2,
                    Digest = new SimpleByte[]
                    {
                        new SimpleByte()
                        {
                            Data = 1
                        }, new SimpleByte()
                        {
                            Data = 2
                        }
                    }
                },
                MessageDynamic = new MessageDynamic()
                {
                    Data = new byte[] { 1, 0, 0, 0, 9, 80, 114, 111, 116, 111, 71, 114, 97, 109 }
                }
            };
        }
    }
}
