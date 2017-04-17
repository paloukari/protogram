using ProtoGram.Protocol.Atp.Encoding;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;

namespace ProtoGram.Protocol.Encoding.Formatter
{
    public class BitEncondingFormatter<T> : IFormatter where T: new()
    {
        private SerializationBinder _binder;
        private StreamingContext _context;
        private ISurrogateSelector _surrogateSelector;

        public SerializationBinder Binder
        {
            get
            {
                return _binder;
            }

            set
            {
                _binder = value;
            }
        }

        public StreamingContext Context
        {
            get
            {
                return _context;
            }

            set
            {
                _context = value;
            }
        }

        public ISurrogateSelector SurrogateSelector
        {
            get
            {
                return _surrogateSelector;
            }

            set
            {
                _surrogateSelector = value;
            }
        }

        public object Deserialize(Stream serializationStream)
        {
            Buffer.BitArray buffer;

            using (StreamReader sr = new StreamReader(serializationStream))
            {
                var data = new byte[serializationStream.Length];
                serializationStream.Read(data, 0, (int)serializationStream.Length);
                buffer = new Buffer.BitArray(data);
            }
            object result = new T();

            DeserializeContractInternal(ref result, buffer);

            return result;
        }
        public void Serialize(Stream serializationStream, object graph)
        {
            Buffer.BitArray buffer = new Buffer.BitArray();

            if (graph == null)
                throw new ArgumentNullException();

            if (!graph.GetType().HasAttribute(typeof(BinaryEncodingContractAttribute)))
                throw new ArgumentException("Needs to be a BinaryEncodingContract type");

            SerializeContractInternal(graph, buffer);
            var bufferData = buffer.ToByteArray();
            serializationStream.Write(bufferData, 0, bufferData.Length);
            serializationStream.Flush();

        }
        private void SerializeContractInternal(object graph, ProtoGram.Protocol.Buffer.BitArray buffer)
        {
            foreach (var member in graph.GetType().GetOrderedSerializableProperties())
            {
                if (member.PropertyType.HasAttribute(typeof(BinaryEncodingContractAttribute)))
                    SerializeContractInternal(member.GetValue(graph), buffer);
                else
                {
                    if (member.PropertyType.IsArray)
                    {
                        if (member.GetPropertyArraySize(graph) == Const.READTOEND)
                        {
                            var rawRata = (byte[])member.GetValue(graph);
                            if (rawRata != null)
                                buffer.Push(rawRata, rawRata.Length * 8);
                        }
                        else
                        {
                            IEnumerable items = member.GetValue(graph) as IEnumerable;
                            foreach (object o in items)
                            {
                                SerializeContractInternal(o, buffer);
                            }
                        }
                    }
                    else
                    {
                        int sizeInBits = member.GetPropertyBitsLength(graph);
                        if (sizeInBits == 0)
                            continue;
                        byte[] bytes;
                        switch (Type.GetTypeCode(member.PropertyType))
                        {
                            case TypeCode.Int32:
                                bytes = Buffer.BitConverter.FromInt32((int)member.GetValue(graph), sizeInBits);
                                break;
                            case TypeCode.Int64:
                                bytes = Buffer.BitConverter.FromInt64((long)member.GetValue(graph), sizeInBits);
                                break;
                            case TypeCode.Byte:
                                bytes = Buffer.BitConverter.FromByte((byte)member.GetValue(graph), sizeInBits);
                                break;
                            default: throw new NotImplementedException();
                        }

                        buffer.Push(bytes, sizeInBits);
                    }
                }
            }
        }


        private void DeserializeContractInternal(ref object graph, Buffer.BitArray buffer)
        {
            foreach (var member in graph.GetType().GetOrderedSerializableProperties())
            {
                if (member.PropertyType.HasAttribute(typeof(BinaryEncodingContractAttribute)))
                {
                    object temp = Activator.CreateInstance(member.PropertyType);
                    DeserializeContractInternal(ref temp, buffer);
                    member.SetValue(graph, temp);
                }
                else
                {
                    object temp;
                    if (member.PropertyType.IsArray)
                    {
                        var length = member.GetPropertyArraySize(graph);
                        if (length == Const.READTOEND)
                        {
                            //read all remaining data
                            byte[] bytes = buffer.Pop(Const.READTOEND);
                            temp = bytes;
                        }
                        else
                        {
                            temp = Activator.CreateInstance(member.PropertyType, new object[] { length });

                            int elementSize = member.PropertyType.GetElementType().GetContractBitsCount(graph);
                            if (elementSize != 0)
                            {
                                var tempArray = (object[])temp;

                                for (int i = 0; i < tempArray.Length; i++)
                                {
                                    var element = Activator.CreateInstance(member.PropertyType.GetElementType());
                                    DeserializeContractInternal(ref element, buffer);
                                    tempArray[i] = element;
                                }
                            }
                        }
                    }
                    else
                    {
                        temp = Activator.CreateInstance(member.PropertyType);

                        int lengthInBits = member.GetPropertyBitsLength(graph);
                        if (lengthInBits == 0)
                            continue;

                        byte[] bytes = buffer.Pop(lengthInBits);

                        switch (Type.GetTypeCode(member.PropertyType))
                        {
                            case TypeCode.Int32:
                                temp = Buffer.BitConverter.ToInt32(bytes);
                                break;
                            case TypeCode.Int64:
                                temp = Buffer.BitConverter.ToInt64(bytes);
                                break;
                            case TypeCode.Byte:
                                temp = Buffer.BitConverter.ToByte(bytes);
                                break;
                            default: throw new NotImplementedException();
                        }
                    }
                    member.SetValue(graph, temp);
                }
            }
        }




    }
}
