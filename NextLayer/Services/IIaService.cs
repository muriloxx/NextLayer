using NextLayer.Models;
using NextLayer.ViewModels; // <-- Adicionado para IaResposta
using System.Collections.Generic; // <-- Adicionado para List
using System.Threading.Tasks;

namespace NextLayer.Services
{
    public interface IIaService
    {
        /// <summary>
        /// Gera uma resposta da IA com base no contexto do chamado e na nova mensagem do cliente.
        /// </summary>
        /// <returns>Um objeto IaResposta contendo o texto, a role sugerida e se deve encaminhar.</returns>
        Task<IaResposta> GerarRespostaAsync(Chamado chamado, string novaMensagemCliente);

        /// <summary>
        /// Compara o problema descrito por um usuário com uma lista de FAQs
        /// e retorna os IDs das FAQs mais relevantes.
        /// </summary>
        /// <param name="tituloProblema">Título digitado pelo usuário.</param>
        /// <param name="descricaoProblema">Descrição digitada pelo usuário.</param>
        /// <param name="faqsExistentes">A lista completa de FAQs do banco.</param>
        /// <returns>Uma lista de IDs de FAQ relevantes.</returns>
        Task<List<int>> SugerirFaqsRelevantesAsync(string tituloProblema, string descricaoProblema, List<FaqItem> faqsExistentes);
    }
}