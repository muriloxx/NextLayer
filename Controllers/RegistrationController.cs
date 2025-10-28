using Microsoft.AspNetCore.Mvc;
using NextLayer.Services;
using NextLayer.ViewModels;
using System;
using System.Threading.Tasks;

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

        [HttpPost("client")]
        public async Task<IActionResult> RegisterClient([FromBody] ClientRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
                

            try
            {
                var newClient = await _authService.RegisterClientAsync(model);
                // Retorna 201 Created com os dados do novo cliente
                return CreatedAtAction(nameof(RegisterClient), new { id = newClient.Id }, newClient);
            }
            catch (Exception ex)
            {   
                // Captura o erro (ex: "E-mail já em uso")
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("employee")]
        public async Task<IActionResult> RegisterEmployee([FromBody] EmployeeRegisterViewModel model)
        {
            // Se o e-mail não for @nextlayer.com, o ModelState será inválido
            // graças ao nosso validador [InstitutionalEmail]
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newEmployee = await _authService.RegisterEmployeeAsync(model);
                return CreatedAtAction(nameof(RegisterEmployee), new { id = newEmployee.Id }, newEmployee);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}