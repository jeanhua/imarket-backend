using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using imarket.models;

namespace imarket.utils
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtTokenGenerator> _logger;

        public JwtTokenGenerator(IConfiguration configuration, ILogger<JwtTokenGenerator> _logger)
        {
            _configuration = configuration;
            this._logger = _logger;
        }

        public TokenModels? GenerateToken(string username, string id, string role)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role,role),
                    new Claim(ClaimTypes.NameIdentifier, id)
                };

                DateTime expires = DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"]));

                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    expires: expires,
                    signingCredentials: credentials);

                var refreshToken = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: new[]{
                    new Claim(JwtRegisteredClaimNames.Sub, id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role,"refresh"),
                    new Claim(ClaimTypes.NameIdentifier, id)
                },
                    expires: DateTime.Now.AddDays(double.Parse(jwtSettings["RefreshTokenExpiresInDays"])),
                    signingCredentials: credentials);

                return new TokenModels
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = new JwtSecurityTokenHandler().WriteToken(refreshToken),
                    Expires = expires,
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in GenerateToken");
                File.AppendAllText("log.txt", DateTime.Now.ToString() + "\t" + e.ToString() + "\n");
                return null;
            }
        }
    }
}
