using NextLayer.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextLayer.Services
{
    /// <summary>
    /// Interface para serviços de agregação de dados e relatórios.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Obtém a contagem de chamados agrupados por status.
        /// </summary>
        /// <returns>Uma lista de StatusReportViewModel.</returns>
        Task<List<StatusReportViewModel>> GetContagemPorStatusAsync();
    }
}