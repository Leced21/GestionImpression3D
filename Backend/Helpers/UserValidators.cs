namespace Backend.Helpers
{
    public static class UserValidators
    {
        private static readonly string[] ValidRoles = { "Admin", "User", "Viewer" };

        public static bool IsValidRole(string role)
        {
            return ValidRoles.Contains(role);
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPassword(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Length >= 6;
        }

        public static string[] GetValidRoles() => ValidRoles;
    }
}
