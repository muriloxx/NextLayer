// --- ARQUIVO: Controllers/AuthController.cs (COM NOVO ENDPOINT DE RESET) ---

using Microsoft.AspNetCore.Mvc;
using NextLayer.Services;
using NextLayer.ViewModels;
using System;
using System.Threading.Tasks;
// Usings necessários para JWT
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims; // <-- Importante para ler o Token
using System.Text;
using NextLayer.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization; // <-- Importante para [Authorize]

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

        // --- MÉTODO DE LOGIN (sem alterações) ---
        [HttpPost("login")]
        [AllowAnonymous] // Login é sempre público
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (userObject, userType) = await _authService.AuthenticateAsync(model);

            if (userObject != null)
            {
                string userName;
                bool isAdmin = false;

                if (userType == "Client")
                {
                    userName = ((Client)userObject).Name;
                }
                else
                {
                    var employee = (Employee)userObject;
                    userName = employee.Name;
                    isAdmin = employee.IsAdmin;
                }

                string token = GerarTokenJwt(userObject, userType, isAdmin);

                return Ok(new
                {
                    message = "Login bem-sucedido!",
                    userType = userType,
                    token = token,
                    userName = userName,
                    isAdmin = isAdmin
                });
            }

            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        // --- MÉTODO MUDAR SENHA (sem alterações) ---
        [HttpPost("mudar-senha")]
        [Authorize] // Apenas usuários logados (Client ou Employee)
        public async Task<IActionResult> MudarSenha([FromBody] MudarSenhaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userType = User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
                {
                    return Unauthorized(new { message = "Token inválido." });
                }

                await _authService.MudarSenhaAsync(userId, userType, model);
                return Ok(new { message = "Senha alterada com sucesso!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, errors = new List<string> { ex.Message } });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Ocorreu um erro interno." });
            }
        }


        // --- INÍCIO DO NOVO MÉTODO ADICIONADO ---

        /// <summary>
        /// (Admin) Força a redefinição da senha de qualquer usuário.
        /// </summary>
        [HttpPost("admin-reset-password")]
        [Authorize(Roles = "Employee")] // 1. Só funcionários podem chamar
        public async Task<IActionResult> AdminResetPassword([FromBody] AdminResetPasswordViewModel model)
        {
            // 2. Verifica se o funcionário logado é um ADMIN
            var isAdminClaim = User.FindFirstValue("isAdmin");
            if (isAdminClaim != "True")
            {
                return StatusCode(403, new { message = "Acesso negado. Apenas administradores podem redefinir senhas." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 3. Chama o serviço que implementamos no Passo 3
                await _authService.AdminResetPasswordAsync(model);

                return Ok(new { message = $"Senha do usuário {model.Email} redefinida com sucesso!" });
            }
            catch (InvalidOperationException ex) // Erros de negócio (ex: "Usuário não encontrado")
            {
                return BadRequest(new { message = ex.Message, errors = new List<string> { ex.Message } });
            }
            catch (Exception) // Outros erros
            {
                return StatusCode(500, new { message = "Ocorreu um erro interno ao redefinir a senha." });
            }
        }

        // --- FIM DO NOVO MÉTODO ADICIONADO ---


        // --- MÉTODO GERAR TOKEN (sem alterações) ---
        private string GerarTokenJwt(object userObject, string userType, bool isAdmin)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key não encontrada");
            var issuer = _configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer não encontrado");
            var audience = _configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience não encontrado");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, (userType == "Client" ? ((Client)userObject).Email : ((Employee)userObject).Email)),
                new Claim(ClaimTypes.NameIdentifier, (userType == "Client" ? ((Client)userObject).Id.ToString() : ((Employee)userObject).Id.ToString())),
                new Claim(ClaimTypes.Role, userType),
                new Claim(ClaimTypes.Name, (userType == "Client" ? ((Client)userObject).Name : ((Employee)userObject).Name)),
                new Claim("isAdmin", isAdmin.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}