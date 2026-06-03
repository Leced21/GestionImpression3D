namespace Backend.DTOs
{
    public class CreateVersionRequest
    {
        public string ChangeLog { get; set; } = string.Empty;
        public bool IsPrototype { get; set; } = false;
    }
}
