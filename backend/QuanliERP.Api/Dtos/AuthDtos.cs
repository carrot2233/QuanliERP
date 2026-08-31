namespace QuanliERP.Api.Dtos
{
    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string CaptchaKey { get; set; } = "";
        public string CaptchaCode { get; set; } = "";
    }

    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string Username { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Role { get; set; } = "";
        public List<string> Permissions { get; set; } = new();
    }
}
