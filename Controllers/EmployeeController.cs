using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Para usar o DbContext diretamente (ou criar um serviço)
using Microsoft.Extensions.Logging;
using NextLayer.Data; // Onde está o AppDbContext
using NextLayer.ViewModels; // Onde está o AnalistaViewModel
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NextLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Roles = "Employee")] // Proteger este endpoint no futuro
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(AppDbContext context, ILogger<EmployeeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retorna uma lista de funcionários (analistas) para preencher dropdowns de designação.
        /// </summary>
        /// <returns>Lista de AnalistaViewModel.</returns>
        [HttpGet("analistas")] // Rota: GET /api/Employee/analistas
        public async Task<IActionResult> GetAnalistasParaDropdown()
        {
            _logger.LogInformation("Buscando lista de analistas para dropdown.");
            try
            {
                var analistas = await _context.Employees
                    // Adicione um filtro aqui se nem todos os Employees são Analistas
                    // Ex: .Where(e => e.Role.Contains("Analista"))
                    .OrderBy(e => e.Name) // Ordena por nome
                    .Select(e => new AnalistaViewModel // Mapeia para o ViewModel
                    {
                        Id = e.Id,
                        Nome = e.Name ?? "Nome não disponível", // Tratamento de nulo
                        Funcao = e.Role ?? "Função não definida" // Usa a propriedade Role como Função
                    })
                    .ToListAsync(); // Executa a consulta

                return Ok(analistas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar lista de analistas.");
                return StatusCode(500, "Erro interno ao buscar analistas.");
            }
        }
    }
}