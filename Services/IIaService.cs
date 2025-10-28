using NextLayer.Models;
using System.Threading.Tasks;

namespace NextLayer.Services
{
    public interface IIaService
    {
        /// <summary>
        /// Gera uma resposta da IA com base no contexto do chamado e na nova mensagem do cliente.
        /// </summary>
        /// <param name="chamado">O chamado completo, incluindo o histórico de mensagens.</param>
        /// <param name="novaMensagemCliente">A nova pergunta do cliente.</param>
        /// <returns>A resposta de texto gerada pela IA.</returns>
        Task<string> GerarRespostaAsync(Chamado chamado, string novaMensagemCliente);
    }
}