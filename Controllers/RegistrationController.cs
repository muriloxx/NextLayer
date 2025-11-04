// --- ARQUIVO: Controllers/RegistrationController.cs (FINALMENTE CORRETO) ---

using Microsoft.AspNetCore.Mvc;
using NextLayer.Services;
using NextLayer.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System; // Adicionado para Exception
using System.Collections.Generic; // Adicionado para List<string>

namespace NextLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly IAuthService _authService;

        public RegistrationController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Endpoint público para clientes se registrarem.
        /// (Agora usa try...catch para ser compatível com seu AuthService)
        /// </summary>
        [HttpPost("client")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterClient([FromBody] ClientRegisterViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // O seu AuthService.cs retorna o Cliente se der certo
                // ou lança uma exceção se der errado.
                var client = await _authService.RegisterClientAsync(model);

                // Se o código chegou aqui, o registro foi um sucesso.
                return StatusCode(201, new { message = "Cliente registrado com sucesso!" });
            }
            catch (InvalidOperationException ex) // Captura erros de negócio (ex: "E-mail já em uso")
            {
                return BadRequest(new { message = "Falha no registro.", errors = new List<string> { ex.Message } });
            }
            catch (Exception ex) // Captura outros erros inesperados
            {
                // Logar o erro (ex.Message) em um sistema de log real
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }


        /// <summary>
        /// Endpoint SEGURO para Administradores registrarem novos funcionários.
        /// </summary>
        [HttpPost("employee")]
        [Authorize(Roles = "Employee")] // 1. Tranca o endpoint
        public async Task<IActionResult> RegisterEmployee([FromBody] EmployeeRegisterViewModel model)
        {
            // 2. Verificação de Admin (lendo o token)
            var isAdminClaim = User.FindFirstValue("isAdmin");

            if (isAdminClaim != "True")
            {
                return StatusCode(403, new { message = "Acesso negado. Apenas administradores podem registrar novos funcionários." });
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // 3. Lógica de try...catch (compatível com seu AuthService)
                var employee = await _authService.RegisterEmployeeAsync(model);

                return StatusCode(201, new { message = "Funcionário registrado com sucesso!" });
            }
            catch (InvalidOperationException ex) // Captura erros de negócio
            {
                return BadRequest(new { message = "Falha no registro.", errors = new List<string> { ex.Message } });
            }
            catch (Exception ex) // Captura outros erros
            {
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }
    }
}