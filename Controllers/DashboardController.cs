using Microsoft.AspNetCore.Authorization; // Para futura autenticação
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NextLayer.Services; // Para IDashboardService
using System;
using System.Threading.Tasks;

namespace NextLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employee")] // IMPORTANTE: Protege o controller, só analistas podem ver
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint para obter estatísticas de chamados por status.
        /// </summary>
        /// <returns>JSON com a lista de status e suas contagens.</returns>
        [HttpGet("stats/por-status")] // Rota: GET /api/Dashboard/stats/por-status
        public async Task<IActionResult> GetStatsPorStatus()
        {
            _logger.LogInformation("Requisição recebida para GetStatsPorStatus.");
            try
            {
                var stats = await _dashboardService.GetContagemPorStatusAsync();
                return Ok(stats); // Retorna 200 OK com os dados
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint GetStatsPorStatus.");
                return StatusCode(500, "Erro interno ao processar estatísticas.");
            }
        }
    }
}