using NextLayer.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
// Adicionamos a referência ao Modelo 'Chamado'
using NextLayer.Models;

namespace NextLayer.Services
{
    public interface IDashboardService
    {
        // --- MÉTODO QUE VOCÊ JÁ TINHA ---
        /// <summary>
        /// Busca no banco e agrupa os chamados por status. (Para o gráfico de pizza)
        /// </summary>
        Task<List<StatusReportViewModel>> GetContagemPorStatusAsync();


        // --- INÍCIO: NOVOS MÉTODOS ADICIONADOS ---

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

        // --- FIM: NOVOS MÉTODOS ADICIONADOS ---
    }
}