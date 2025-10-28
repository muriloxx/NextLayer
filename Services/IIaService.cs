using NextLayer.Models;
using NextLayer.ViewModels; // Adicionado
using System.Threading.Tasks;

namespace NextLayer.Services
{
    /// <summary>
    /// Representa a resposta estruturada da IA (texto + ação).
    /// </summary>
    public class IaResposta
    {
        public string TextoResposta { get; set; } = string.Empty;
        public string? RoleSugerida { get; set; }
        public bool DeveEncaminhar { get; set; }
    }

    /// <summary>
    /// Interface para o serviço de chat com IA
    /// </summary>
    public interface IIaService
    {
        /// <summary>
        /// Gera uma resposta da IA com base no contexto do chamado.
        /// </summary>
        /// <returns>Um objeto IaResposta contendo o texto e a role sugerida.</returns>
        Task<IaResposta> GerarRespostaAsync(Chamado chamado, string novaMensagemCliente);
    }
}