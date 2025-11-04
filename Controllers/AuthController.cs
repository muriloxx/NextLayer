using Microsoft.AspNetCore.Mvc;
using NextLayer.Services;
using NextLayer.ViewModels;
using System;
using System.Threading.Tasks;
// Usings necessários para JWT
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NextLayer.Models; // Necessário para Client e Employee
using System.Collections.Generic; // Para List<Claim>

namespace NextLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        // Construtor (sem alterações)
        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // O AuthService retorna o objeto (Client ou Employee) e o tipo
            var (userObject, userType) = await _authService.AuthenticateAsync(model);

            if (userObject != null)
            {

                string userName;
                bool isAdmin = false; // Padrão é falso (para clientes)

                // Define userName e isAdmin com base no tipo de usuário
                if (userType == "Client")
                {
                    userName = ((Client)userObject).Name;
                    // isAdmin continua false
                }
                else // userType == "Employee"
                {
                    var employee = (Employee)userObject;
                    userName = employee.Name;
                    isAdmin = employee.IsAdmin; // <--- LÊ A NOVA PROPRIEDADE
                }

                // Gera o token JWT, agora passando o status de admin
                string token = GerarTokenJwt(userObject, userType, isAdmin); // <--- PARÂMETRO ADICIONADO

                // Retorna o token, o tipo, o nome E o status de admin
                return Ok(new
                {
                    message = "Login bem-sucedido!",
                    userType = userType,
                    token = token,
                    userName = userName,
                    isAdmin = isAdmin // <--- VALOR ADICIONADO À RESPOSTA
                });

            }

            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        // Método privado para gerar o token JWT (agora recebe 'isAdmin')
        private string GerarTokenJwt(object userObject, string userType, bool isAdmin)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key não encontrada");
            var issuer = _configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer não encontrado");
            var audience = _configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience não encontrado");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Claims (informações dentro do token)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, (userType == "Client" ? ((Client)userObject).Email : ((Employee)userObject).Email)),
                // ClaimTypes.NameIdentifier é o padrão para ID do usuário
                new Claim(ClaimTypes.NameIdentifier, (userType == "Client" ? ((Client)userObject).Id.ToString() : ((Employee)userObject).Id.ToString())),
                // ClaimTypes.Role é usado pelo [Authorize(Roles = "...")]
                new Claim(ClaimTypes.Role, userType),
                // ClaimTypes.Name é o padrão para o nome
                new Claim(ClaimTypes.Name, (userType == "Client" ? ((Client)userObject).Name : ((Employee)userObject).Name)),
                
                // Adicionamos a claim "isAdmin" ao token.
                // O .ToString() converte o booleano (true/false) para a string "True"/"False"
                new Claim("isAdmin", isAdmin.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(8), // Duração do token
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}