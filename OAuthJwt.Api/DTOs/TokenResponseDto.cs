namespace OAuthJwt.Api.DTOs
{
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
