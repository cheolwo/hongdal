using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using 홍달.Data;

namespace Hongdal.Security
{
    public sealed class AuthTokenService
    {
        private readonly JwtOptions _jwtOptions;

        public AuthTokenService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        public string CreateAccessToken(ApplicationUser user, IEnumerable<string> roles, out DateTime expiresAtUtc)
        {
            var now = DateTime.UtcNow;
            expiresAtUtc = now.AddMinutes(_jwtOptions.AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id)
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: now,
                expires: expiresAtUtc,
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var buffer = new byte[64];
            RandomNumberGenerator.Fill(buffer);
            return Convert.ToBase64String(buffer);
        }

        public string HashRefreshToken(string refreshToken)
        {
            var bytes = Encoding.UTF8.GetBytes(refreshToken);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }

        public bool VerifyRefreshToken(string refreshToken, string expectedHash)
        {
            var actualHash = HashRefreshToken(refreshToken);
            var actualBytes = Encoding.UTF8.GetBytes(actualHash);
            var expectedBytes = Encoding.UTF8.GetBytes(expectedHash);
            return CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
        }
    }
}
