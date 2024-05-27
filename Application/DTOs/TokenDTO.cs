using Domain.Entities.Users;

namespace AmxBookstore.Application.DTOs
{
    public class TokenDTO
    {
        public string Id {  get; set; }
        public string Role { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
