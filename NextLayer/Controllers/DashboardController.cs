using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NextLayer.Services;
using System;
using System.Threading.Tasks;
using NextLayer.Models;
using System.Collections.Generic;

namespace NextLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employee")] // Seu controller já estava corretamente protegido
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
        /// Endpoint para obter estatísticas de chamados por status. (Para o gráfico de pizza)
        /// (Este endpoint já existia no seu arquivo)
        /// </summary>
        [HttpGet("stats/por-status")] // Rota: GET /api/Dashboard/stats/por-status
        public async Task<IActionResult> GetStatsPorStatus()
        {
            _logger.LogInformation("Requisição recebida para GetStatsPorStatus.");
            try
            {
                var stats = await _dashboardService.GetContagemPorStatusAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint GetStatsPorStatus.");
                return StatusCode(500, "Erro interno ao processar estatísticas.");
            }
        }


        // --- INÍCIO: ENDPOINTS  ---

        /// <summary>
        /// Relatório 1: Retorna o número total de chamados abertos.
        /// </summary>
        [HttpGet("total-abertos")] // Rota: GET /api/dashboard/total-abertos
        public async Task<ActionResult<int>> GetTotalChamadosAbertos()
        {
            try
            {
                var total = await _dashboardService.GetTotalChamadosAbertosAsync();
                return Ok(total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint GetTotalChamadosAbertos.");
                return StatusCode(500, new { message = $"Erro interno: {ex.Message}" });
            }
        }

        /// <summary>
        /// Relatório 2: Retorna a contagem de chamados abertos agrupados por prioridade.
        /// </summary>
        [HttpGet("por-prioridade")] // Rota: GET /api/dashboard/por-prioridade
        public async Task<ActionResult<Dictionary<string, int>>> GetChamadosPorPrioridade()
        {
            try
            {
                var data = await _dashboardService.GetChamadosAbertosPorPrioridadeAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint GetChamadosPorPrioridade.");
                return StatusCode(500, new { message = $"Erro interno: {ex.Message}" });
            }
        }

        /// <summary>
        /// Relatório 3: Retorna uma lista de chamados abertos recentemente.
        /// </summary>
        [HttpGet("recentes")] // Rota: GET /api/dashboard/recentes
        public async Task<ActionResult<IEnumerable<Chamado>>> GetChamadosRecentes([FromQuery] int dias = 7)
        {
            try
            {
                var chamados = await _dashboardService.GetChamadosAbertosRecentementeAsync(dias);
                return Ok(chamados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint GetChamadosRecentes.");
                return StatusCode(500, new { message = $"Erro interno: {ex.Message}" });
            }
        }

        // --- FIM: ENDPOINTS  ---
    }
}