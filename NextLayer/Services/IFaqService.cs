using NextLayer.Models;
using NextLayer.ViewModels; // Supondo que você possa ter ViewModels para FAQ no futuro
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextLayer.Services
{
    /// <summary>
    /// Interface para o serviço que gerencia os itens da Base de Conhecimento (FAQ).
    /// </summary>
    public interface IFaqService
    {
        /// <summary>
        /// Obtém todos os itens de FAQ do banco de dados.
        /// </summary>
        /// <returns>Uma lista de FaqItem.</returns>
        Task<List<FaqItem>> GetAllFaqsAsync();

        /// <summary>
        /// Sugere itens de FAQ relevantes com base em um título e descrição.
        /// (Implementação inicial usará busca simples, futuramente pode usar IA).
        /// </summary>
        /// <param name="titulo">Título do problema.</param>
        /// <param name="descricao">Descrição do problema.</param>
        /// <returns>Uma lista de FaqItem sugeridos.</returns>
        Task<List<FaqItem>> GetFaqSugestoesAsync(string titulo, string descricao);

        /// <summary>
        /// Adiciona os exemplos de FAQ iniciais ao banco de dados, se ele estiver vazio.
        /// </summary>
        Task SeedInitialFaqsAsync();
    }
}