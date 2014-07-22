namespace DocLite.Serialization
{
    public interface IDocumentSerializer
    {
        string Serialize<T>(T graph);
        T Deserialize<T>(string value);
    }
}