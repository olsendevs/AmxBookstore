using AmxBookstore.Application.DTOs;

namespace AmxBookstore.Application.Interfaces
{
    public interface IAuthService
    {
        Task<TokenDTO> LoginAsync(LoginDTO loginDTO);
        Task<TokenDTO> RefreshTokenAsync(string refreshToken);
    }
}
