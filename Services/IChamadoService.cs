using NextLayer.Models;
using NextLayer.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextLayer.Services
{
    public interface IChamadoService
    {
        // Alterado: Agora retorna o ViewModel de detalhe
        Task<DetalheChamadoViewModel> CriarNovoChamado(CriarChamadoViewModel model, int clienteId);

        Task<IEnumerable<ChamadoGridViewModel>> GetChamadosEmAberto();

        // Alterado: Agora retorna o ViewModel de detalhe
        Task<DetalheChamadoViewModel> GetDetalheChamado(int chamadoId);

        Task<Chamado> AtualizarChamado(int chamadoId, AtualizarChamadoViewModel model);

        // --- (NOVOS MÉTODOS - Parte 2) ---
        Task<IEnumerable<ChamadoGridViewModel>> GetChamadosPorCliente(int clienteId);
        Task<List<MensagemViewModel>> AdicionarMensagem(int chamadoId, string conteudo, int remetenteId, string tipoRemetente);
    }
}