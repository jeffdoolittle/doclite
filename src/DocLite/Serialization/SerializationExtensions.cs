using System.IO;

namespace DocLite.Serialization
{
    public static class SerializationExtensions
    {
        public static byte[] Serialize<T>(this ISerialize serializer, T value)
        {
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, value);
                return stream.ToArray();
            }
        }

        public static T Deserialize<T>(this ISerialize serializer, byte[] serialized)
        {
            serialized = serialized ?? new byte[] { };
            if (serialized.Length == 0)
            {
                return default(T);
            }

            using (var stream = new MemoryStream(serialized))
                return serializer.Deserialize<T>(stream);
        }
    }
}