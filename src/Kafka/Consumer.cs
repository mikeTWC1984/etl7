
using System;
using Confluent.Kafka;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nest.Utf8Json;

namespace ETL.Kafka
{
    public static class KafkaFactory

    {

        private static ConsumerConfig GetConsumerConfig(IDictionary conf)
        {
            var dict = new Dictionary<String, String>();
            foreach (var key in conf.Keys)
            {
                dict.Add(key as String, conf[key] as String);
            }
            return new ConsumerConfig(dict);
        }

        private static ConsumerConfig GetConsumerConfig(String cnString)
        {
            var dict = cnString.Split(';')
            .Select(value => value.Split('='))
            .ToDictionary(pair => pair[0], pair => pair[1]);
            return new ConsumerConfig(dict);
        }

        /// <summary>
        ///  STRING
        /// </summary>
        /// <param name="conf"></param>
        /// <returns></returns> 
        public static IConsumer<Ignore, String> GetConsumer(ConsumerConfig conf)
        {
            return (new ConsumerBuilder<Ignore, String>(conf)).Build();
        }

        /// <summary> Create Consumer from IDictionary (meant for PowerShell Hashtable) </summary>
        public static IConsumer<Ignore, String> GetConsumer(IDictionary conf)
        {
            return GetConsumer(GetConsumerConfig(conf));
        }

        /// <summary> Create Consumer from config string, e.g. "group.id=test;bootstrap.servers=localhost:9092"</summary>
        public static IConsumer<Ignore, String> GetConsumer(String conf)
        {
            return GetConsumer(GetConsumerConfig(conf));
        }


        /// <summary>
        ///  BYTE
        /// </summary>
        /// <param name="conf"></param>
        /// <returns></returns> 
        public static IConsumer<Ignore, Byte[]> GetByteConsumer(ConsumerConfig conf)
        {
            return (new ConsumerBuilder<Ignore, Byte[]>(conf)).Build();
        }

        public static IConsumer<Ignore, Byte[]> GetByteConsumer(IDictionary conf)
        {
            return GetByteConsumer(GetConsumerConfig(conf));
        }

        public static IConsumer<Ignore, Byte[]> GetByteConsumer(String conf)
        {
            return GetByteConsumer(GetConsumerConfig(conf));
        }

        /// <summary>
        ///  JSON
        /// </summary>
        /// <param name="conf"></param>
        /// <returns></returns> 

        public class Utf8JsonSerDe : ISerializer<Object>, IDeserializer<Object>
        {
            public byte[] Serialize(Object data, SerializationContext context)
            {
                return System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(data));

            }

            public Object Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
            {
                return JsonSerializer.Deserialize<dynamic>(data.ToArray());
            }

        }

        public static IConsumer<Ignore, Object> GetJsonConsumer(ConsumerConfig conf)
        {
            var consumer = new ConsumerBuilder<Ignore, Object>(conf)
                .SetValueDeserializer(new Utf8JsonSerDe())
                .Build();
            return consumer;
        }

        public static IConsumer<Ignore, Object> GetJsonConsumer(IDictionary conf)
        {
            return GetJsonConsumer(GetConsumerConfig(conf));
        }

        public static IConsumer<Ignore, Object> GetJsonConsumer(String conf)
        {
            return GetJsonConsumer(GetConsumerConfig(conf));
        }



/// <summary>
///  HELPERS
/// </summary>
/// <param name="count"></param>
/// <param name="consumer"></param>
/// <param name="timeout"></param>
/// <returns></returns>
        public static List<ConsumeResult<Ignore, String>> ReadNext(int count, IConsumer<Ignore, String> consumer, int timeout = 1000)
        {

            var data = new List<ConsumeResult<Ignore, String>>();

            for (var i = 0; i < count; i++)
            {
                var msg = consumer.Consume(timeout);
                if (msg is null) break;
                data.Add(msg);

            }

            return data;

        }

        public static List<ConsumeResult<Ignore, Byte>> ReadNext(int count, IConsumer<Ignore, Byte> consumer, int timeout = 1000)
        {

            var data = new List<ConsumeResult<Ignore, Byte>>();

            for (var i = 0; i < count; i++)
            {
                var msg = consumer.Consume(timeout);
                if (msg is null) break;
                data.Add(msg);

            }

            return data;

        }

        public static List<ConsumeResult<Ignore, Object>> ReadNext(int count, IConsumer<Ignore, Object> consumer, int timeout = 1000)
        {

            var data = new List<ConsumeResult<Ignore, Object>>();

            for (var i = 0; i < count; i++)
            {
                var msg = consumer.Consume(timeout);
                if (msg is null) break;
                data.Add(msg);

            }

            return data;

        }




    }





}