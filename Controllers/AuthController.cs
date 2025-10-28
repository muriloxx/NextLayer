using Microsoft.AspNetCore.Mvc;
using NextLayer.Services;
using NextLayer.ViewModels;
using System.Threading.Tasks;

namespace NextLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            // A validação do [Required] etc. é feita automaticamente pelo [ApiController]
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (user, userType) = await _authService.AuthenticateAsync(model);

            if (user != null)
            {
                // Sucesso! Em um app real, você geraria um Token JWT aqui.
                return Ok(new
                {
                    message = "Login bem-sucedido!",
                    userType = userType,
                    userData = user
                });
            }

            // Falha na autenticação
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }
    }
}