namespace Backend.Interface
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? UserEmail { get; }
        string? IpAddress { get; }
    }
}
