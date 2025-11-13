using NextLayer.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextLayer.Models;

namespace NextLayer.Services
{
    public interface IDashboardService
    {
        /// <summary>
        /// Busca no banco e agrupa os chamados por status. (Para o gráfico de pizza)
        /// </summary>
        Task<List<StatusReportViewModel>> GetContagemPorStatusAsync();



        /// <summary>
        /// Relatório 1: Retorna o número total de chamados abertos.
        /// </summary>
        Task<int> GetTotalChamadosAbertosAsync();

        /// <summary>
        /// Relatório 2: Retorna a contagem de chamados abertos agrupados por prioridade.
        /// </summary>
        Task<Dictionary<string, int>> GetChamadosAbertosPorPrioridadeAsync();

        /// <summary>
        /// Relatório 3: Retorna uma lista de chamados abertos recentemente.
        /// </summary>
        /// <param name="diasRecentes">O número de dias para considerar "recente". O padrão é 7.</param>
        Task<List<Chamado>> GetChamadosAbertosRecentementeAsync(int diasRecentes = 7);

    }
}