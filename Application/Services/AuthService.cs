using AmxBookstore.Application.DTOs;
using AmxBookstore.Application.Interfaces;
using AmxBookstore.Application.Models;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AmxBookstore.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            UserManager<User> userManager,
            IConfiguration configuration,
            ILogger logger,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<TokenDTO> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDTO.Password))
            {
                _logger.Warning("Invalid login attempt for user: {Email}", loginDTO.Email);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any())
            {
                _logger.Warning("No roles assigned for user: {Email}", loginDTO.Email);
                throw new UnauthorizedAccessException("User does not have any roles assigned");
            }

            return await GenerateTokensAsync(user, roles);
        }

        public async Task<TokenDTO> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.Warning("Invalid or expired refresh token: {RefreshToken}", refreshToken);
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any())
            {
                _logger.Warning("No roles assigned for user: {UserId}", user.Id);
                throw new UnauthorizedAccessException("User does not have any roles assigned");
            }

            return await GenerateTokensAsync(user, roles);
        }

        private async Task<TokenDTO> GenerateTokensAsync(User user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accessToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: creds);

            var accessTokenStr = new JwtSecurityTokenHandler().WriteToken(accessToken);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            await _userManager.UpdateAsync(user);

            return new TokenDTO
            {
                Id = user.Id.ToString(),
                Role = user.Role.ToString(),
                AccessToken = accessTokenStr,
                RefreshToken = refreshToken
            };
        }


        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
