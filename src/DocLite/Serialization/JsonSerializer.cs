using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DocLite.Serialization
{
    public class JsonSerializer : ISerialize
    {
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings();

        public JsonSerializer()
            : this(cfg =>
                {
                    var contractResolver = new DefaultContractResolver();
                    contractResolver.DefaultMembersSearchFlags |= BindingFlags.NonPublic;
                    cfg.TypeNameHandling = TypeNameHandling.All;
                    cfg.TypeNameAssemblyFormat = FormatterAssemblyStyle.Full;
                    cfg.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    cfg.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    cfg.ContractResolver = contractResolver;
                })
        {
        }

        public JsonSerializer(Action<JsonSerializerSettings> configure)
        {
            configure(_serializerSettings);
        }

        public virtual void Serialize<T>(Stream output, T graph)
        {
            using (var streamWriter = new StreamWriter(output, Encoding.UTF8))
                Serialize(new JsonTextWriter(streamWriter), graph);
        }

        public virtual T Deserialize<T>(Stream input)
        {
            using (var streamReader = new StreamReader(input, Encoding.UTF8))
                return Deserialize<T>(new JsonTextReader(streamReader));
        }

        protected virtual void Serialize(JsonWriter writer, object graph)
        {
            using (writer)
                GetSerializer(graph.GetType()).Serialize(writer, graph);
        }

        protected virtual T Deserialize<T>(JsonReader reader)
        {
            Type type = typeof(T);

            using (reader)
                return (T)GetSerializer(type).Deserialize(reader, type);
        }

        protected virtual Newtonsoft.Json.JsonSerializer GetSerializer(Type typeToSerialize)
        {
            return Newtonsoft.Json.JsonSerializer.Create(_serializerSettings);
        }
    }
}