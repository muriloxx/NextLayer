using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NextLayer.Data;
using NextLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Adicionamos a referência ao Modelo 'Chamado'
using NextLayer.Models;

namespace NextLayer.Services
{
    /// <summary>
    /// Implementação do serviço de dashboard, busca e agrupa dados de chamados.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        // Lista de status que consideramos como "não abertos"
        // Baseado na lógica encontrada em seu ChamadoService
        private readonly string[] statusFechados = { "Concluído", "Encerrado", "Cancelado" };

        public DashboardService(AppDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Implementação do método que busca no banco e agrupa os chamados por status.
        /// (Este método já existia no seu arquivo)
        /// </summary>
        public async Task<List<StatusReportViewModel>> GetContagemPorStatusAsync()
        {
            _logger.LogInformation("Gerando relatório de contagem de chamados por status.");
            try
            {
                var contagemPorStatus = await _context.Chamados
                    .GroupBy(c => c.Status)
                    .Select(g => new StatusReportViewModel
                    {
                        Status = g.Key ?? "Sem Status",
                        Contagem = g.Count()
                    })
                    .OrderByDescending(r => r.Contagem)
                    .ToListAsync();

                return contagemPorStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório de contagem por status.");
                return new List<StatusReportViewModel>();
            }
        }


        // --- INÍCIO: IMPLEMENTAÇÃO DOS NOVOS MÉTODOS ---

        /// <summary>
        /// Relatório 1: Retorna o número total de chamados abertos.
        /// </summary>
        public async Task<int> GetTotalChamadosAbertosAsync()
        {
            _logger.LogInformation("Buscando contagem total de chamados abertos.");
            try
            {
                // Conta todos os chamados cujo status NÃO ESTÁ na lista de statusFechados
                return await _context.Chamados
                    .CountAsync(c => !statusFechados.Contains(c.Status));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar contagem total de chamados abertos.");
                return 0;
            }
        }

        /// <summary>
        /// Relatório 2: Retorna a contagem de chamados abertos agrupados por prioridade.
        /// </summary>
        public async Task<Dictionary<string, int>> GetChamadosAbertosPorPrioridadeAsync()
        {
            _logger.LogInformation("Buscando contagem de chamados por prioridade.");
            try
            {
                return await _context.Chamados
                    .Where(c => !statusFechados.Contains(c.Status) && c.Prioridade != null) // Filtra apenas abertos
                    .GroupBy(c => c.Prioridade) // Agrupa por Prioridade
                    .Select(g => new { Prioridade = g.Key, Total = g.Count() })
                    .ToDictionaryAsync(k => k.Prioridade, v => v.Total); // Converte para Dicionário
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar contagem por prioridade.");
                return new Dictionary<string, int>(); // Retorna dicionário vazio em caso de erro
            }
        }

        /// <summary>
        /// Relatório 3: Retorna uma lista de chamados abertos recentemente.
        /// </summary>
        public async Task<List<Chamado>> GetChamadosAbertosRecentementeAsync(int diasRecentes = 7)
        {
            _logger.LogInformation("Buscando chamados abertos nos últimos {diasRecentes} dias.", diasRecentes);
            try
            {
                // Define a data de corte (hoje - X dias)
                var dataLimite = DateTime.UtcNow.AddDays(-diasRecentes);

                return await _context.Chamados
                    .Where(c => !statusFechados.Contains(c.Status) && c.DataAbertura >= dataLimite) // Filtra abertos E recentes
                    .OrderByDescending(c => c.DataAbertura) // Mais novos primeiro
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar chamados recentes.");
                return new List<Chamado>(); // Retorna lista vazia em caso de erro
            }
        }

        // --- FIM: IMPLEMENTAÇÃO DOS NOVOS MÉTODOS ---
    }
}