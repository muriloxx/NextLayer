using NextLayer.Models;
using NextLayer.ViewModels; // Garanta que este using existe
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextLayer.Services
{
    public interface IChamadoService
    {
        Task<DetalheChamadoViewModel> CriarNovoChamado(CriarChamadoViewModel model, int clienteId);
        Task<IEnumerable<ChamadoGridViewModel>> GetChamadosEmAberto();
        Task<IEnumerable<ChamadoGridViewModel>> GetChamadosPorCliente(int clienteId);
        Task<DetalheChamadoViewModel> GetDetalheChamado(int chamadoId);
        Task<Chamado> AtualizarChamado(int chamadoId, AtualizarChamadoViewModel model);
        Task<List<MensagemViewModel>> AdicionarMensagem(int chamadoId, string conteudo, int remetenteId, string tipoRemetente);

        // --- MÉTODO ADICIONADO QUE ESTAVA FALTANDO ---
        /// <summary>
        /// Obtém os chamados não concluídos/encerrados atribuídos a um analista específico.
        /// </summary>
        /// <param name="analistaId">ID do funcionário (analista).</param>
        /// <returns>Lista de ChamadoGridViewModel.</returns>
        Task<IEnumerable<ChamadoGridViewModel>> GetChamadosPorAnalistaAsync(int analistaId);
        // --- FIM DA ADIÇÃO ---
    }
}