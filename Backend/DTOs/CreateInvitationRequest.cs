namespace Backend.DTOs
{
    public class CreateInvitationRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }
}
