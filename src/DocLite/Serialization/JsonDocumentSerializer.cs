using System.Text;

namespace DocLite.Serialization
{
    public class JsonDocumentSerializer : IDocumentSerializer
    {
        private readonly JsonSerializer _serializer;

        public JsonDocumentSerializer(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public string Serialize<T>(T graph)
        {
            return Encoding.UTF8.GetString(_serializer.Serialize(graph));
        }

        public T Deserialize<T>(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            return _serializer.Deserialize<T>(bytes);
        }
    }
}