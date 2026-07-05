namespace Backend.Interface
{
    public interface IAuditSerializer
    {
        string Serialize<T>(T obj);
        T? Deserialize<T>(string json);
    }
}
