using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OAuthJwt.Api.DTOs;

namespace OAuthJwt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint para autenticar usuarios y generar Token JWT independiente.
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validación de credenciales de ejemplo / base
            // En producción puede validar contra BD o Azure SQL
            string role = "User";
            bool isValid = false;

            if (dto.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) && dto.Password == "Admin123*")
            {
                role = "Administrador";
                isValid = true;
            }
            else if (dto.Username.Equals("operador", StringComparison.OrdinalIgnoreCase) && dto.Password == "Operador123*")
            {
                role = "Operador";
                isValid = true;
            }
            else if (dto.Username.Equals("docente", StringComparison.OrdinalIgnoreCase) && dto.Password == "Docente2026*")
            {
                role = "Evaluador";
                isValid = true;
            }

            if (!isValid)
            {
                _logger.LogWarning("Intento de inicio de sesión fallido para el usuario: {Username}", dto.Username);
                return Unauthorized(new 
                { 
                    mensaje = "Credenciales incorrectas. Verifique el usuario y contraseña.",
                    status = 401 
                });
            }

            var tokenResponse = GenerarTokenJwt(dto.Username, role);
            _logger.LogInformation("Token JWT emitido exitosamente para usuario: {Username}, Rol: {Role}", dto.Username, role);

            return Ok(tokenResponse);
        }

        /// <summary>
        /// Endpoint de verificación de estado y parámetros del servicio OAuth/JWT.
        /// </summary>
        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(new
            {
                Servicio = "OAuthJWT Microservicio Independiente",
                Estado = "Activo / Saludable",
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                ExpiracionMinutos = _configuration["Jwt:ExpiresInMinutes"],
                FechaServidorUtc = DateTime.UtcNow
            });
        }

        private TokenResponseDto GenerarTokenJwt(string username, string role)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "SuperSecretKeyForDistributedSystemsVehiculos2026!#";
            var issuer = _configuration["Jwt:Issuer"] ?? "OAuthJwtService";
            var audience = _configuration["Jwt:Audience"] ?? "SistemaDistribuidosVehiculos";
            var expireMinutes = int.TryParse(_configuration["Jwt:ExpiresInMinutes"], out int exp) ? exp : 60;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var issuedAt = DateTime.UtcNow;
            var expiresAt = issuedAt.AddMinutes(expireMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(issuedAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim("sistema", "SistemaDistribuidos_Vehiculos")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: issuedAt,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new TokenResponseDto
            {
                AccessToken = tokenString,
                TokenType = "Bearer",
                ExpiresIn = expireMinutes * 60,
                Username = username,
                Role = role,
                IssuedAt = issuedAt,
                ExpiresAt = expiresAt
            };
        }
    }
}
