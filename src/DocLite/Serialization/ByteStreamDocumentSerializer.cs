using System;

namespace DocLite.Serialization
{
    public class ByteStreamDocumentSerializer : IDocumentSerializer
    {
        private readonly ISerialize _serializer;

        public ByteStreamDocumentSerializer(ISerialize serializer)
        {
            _serializer = serializer;
        }

        public string Serialize<T>(T graph)
        {
            var bytes = _serializer.Serialize(graph);
            return ToBase64(bytes);
        }

        public T Deserialize<T>(string value)
        {
            byte[] bytes = FromBase64(value) ?? new byte[0];
            return _serializer.Deserialize<T>(bytes);
        }

        private static string ToBase64(byte[] value)
        {
            if (value == null)
            {
                return null;
            }

            try
            {
                return Convert.ToBase64String(value);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private static byte[] FromBase64(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            try
            {
                return Convert.FromBase64String(value);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}