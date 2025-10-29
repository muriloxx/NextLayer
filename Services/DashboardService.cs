using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NextLayer.Data; // Para AppDbContext
using NextLayer.ViewModels; // Para StatusReportViewModel
using System;
using System.Collections.Generic;
using System.Linq; // Para GroupBy e Select
using System.Threading.Tasks;

namespace NextLayer.Services
{
    /// <summary>
    /// Implementação do serviço de dashboard, busca e agrupa dados de chamados.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(AppDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Implementação do método que busca no banco e agrupa os chamados por status.
        /// </summary>
        public async Task<List<StatusReportViewModel>> GetContagemPorStatusAsync()
        {
            _logger.LogInformation("Gerando relatório de contagem de chamados por status.");
            try
            {
                // Usamos o DbContext para acessar a tabela Chamados
                var contagemPorStatus = await _context.Chamados
                    .GroupBy(c => c.Status) // Agrupa todos os chamados pelo campo "Status"
                    .Select(g => new StatusReportViewModel // Cria um novo ViewModel para cada grupo
                    {
                        Status = g.Key ?? "Sem Status", // g.Key é o valor pelo qual agrupamos (o Status)
                        Contagem = g.Count() // Conta quantos itens há em cada grupo
                    })
                    .OrderByDescending(r => r.Contagem) // Ordena do mais comum para o menos comum
                    .ToListAsync(); // Executa a consulta no banco

                return contagemPorStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório de contagem por status.");
                return new List<StatusReportViewModel>(); // Retorna lista vazia em caso de erro
            }
        }
    }
}