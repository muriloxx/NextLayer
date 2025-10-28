// --- ARQUIVO: Controllers/AuthController.cs (COMPLETO E ATUALIZADO) ---

using Microsoft.AspNetCore.Mvc;
using NextLayer.Services;
using NextLayer.ViewModels;
using System;
using System.Threading.Tasks;
// --- USINGS ADICIONADOS PARA JWT ---
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NextLayer.Models; // Necessário para pegar o ID e Role
// --- FIM USINGS ---

namespace NextLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration; // Injeta IConfiguration para ler a chave JWT

        public AuthController(IAuthService authService, IConfiguration configuration) // Construtor atualizado
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // O AuthService agora retorna (objeto user, string userType)
            var (userObject, userType) = await _authService.AuthenticateAsync(model);

            if (userObject != null)
            {
                // --- GERAÇÃO DO TOKEN ---
                string token = GerarTokenJwt(userObject, userType);
                // --- FIM DA GERAÇÃO ---

                // Retorna o token e o tipo de usuário para o front-end
                return Ok(new
                {
                    message = "Login bem-sucedido!",
                    userType = userType,
                    token = token // Envia o token para o front-end
                });
            }

            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        // --- NOVO MÉTODO PRIVADO PARA GERAR O TOKEN ---
        private string GerarTokenJwt(object userObject, string userType)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key não encontrada");
            var issuer = _configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer não encontrado");
            var audience = _configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience não encontrado");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Claims são as "informações" dentro do token
            var claims = new List<Claim>
            {
                // Adiciona o Email
                new Claim(JwtRegisteredClaimNames.Email, (userType == "Client" ? ((Client)userObject).Email : ((Employee)userObject).Email)),
                // Adiciona o ID (NameIdentifier é o padrão para ID)
                new Claim(ClaimTypes.NameIdentifier, (userType == "Client" ? ((Client)userObject).Id.ToString() : ((Employee)userObject).Id.ToString())),
                // Adiciona a Role (Função)
                new Claim(ClaimTypes.Role, userType) // "Client" ou "Employee"
            };

            // (Opcional) Adiciona o Nome
            claims.Add(new Claim(ClaimTypes.Name, (userType == "Client" ? ((Client)userObject).Name : ((Employee)userObject).Name)));

            // Cria o token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(8), // Token expira em 8 horas
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        // --- FIM DO NOVO MÉTODO ---
    }
}