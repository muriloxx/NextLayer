using Microsoft.EntityFrameworkCore;
using NextLayer.Data;
using NextLayer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLayer.Services
{
    // Esta classe implementa a IFaqService
    public class FaqService : IFaqService
    {
        private readonly AppDbContext _context;

        public FaqService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Implementação de: Obtém todos os itens de FAQ do banco de dados.
        /// </summary>
        public async Task<List<FaqItem>> GetAllFaqsAsync()
        {

            return await _context.FaqItens.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Implementação de: Sugere itens de FAQ relevantes.
        /// </summary>
        public async Task<List<FaqItem>> GetFaqSugestoesAsync(string titulo, string descricao)
        {
            var palavrasChaveDescricao = descricao.Split(' ')
                                                  .Select(p => p.ToLower())
                                                  .Where(p => p.Length > 3);

            var palavrasChaveTitulo = titulo.Split(' ')
                                            .Select(p => p.ToLower())
                                            .Where(p => p.Length > 3);

            var palavrasChave = palavrasChaveDescricao.Union(palavrasChaveTitulo);


            var todosFaqs = await _context.FaqItens.AsNoTracking().ToListAsync();

            var sugestoes = todosFaqs
                .Where(faq => palavrasChave.Any(p => faq.Pergunta.ToLower().Contains(p) ||
                                                     faq.Resposta.ToLower().Contains(p)))
                .Take(3) // Limita a 3 sugestões
                .ToList();

            return sugestoes;
        }

        /// <summary>
        /// Implementação de: Adiciona FAQs iniciais ao banco de dados.
        /// </summary>
        public async Task SeedInitialFaqsAsync()
        {

            if (await _context.FaqItens.AnyAsync())
            {
                // Se o banco já tiver FAQs, não faz nada.
                return;
            }

            var listaFaqs = new List<FaqItem>
            {
                new FaqItem
                {
                    Pergunta = "Como redefinir minha senha?",
                    Resposta = "Para redefinir sua senha, vá até a tela de login e clique em 'Esqueci minha senha'. Siga as instruções enviadas para o seu e-mail.",
                    DataCriacao = DateTime.UtcNow
                },
                new FaqItem
                {
                    Pergunta = "Não consigo acessar o sistema, o que fazer?",
                    Resposta = "Verifique se seu usuário e senha estão corretos. Se o problema persistir, tente redefinir sua senha ou entre em contato com o administrador.",
                    DataCriacao = DateTime.UtcNow
                },
                new FaqItem
                {
                    Pergunta = "Como abrir um novo chamado?",
                    Resposta = "Na sua tela principal (Dashboard), clique no botão 'Novo Chamado'. Preencha o formulário com o máximo de detalhes possível, incluindo título, descrição e categoria.",
                    DataCriacao = DateTime.UtcNow
                }
            };

            // Adiciona os FAQs iniciais ao contexto
            await _context.FaqItens.AddRangeAsync(listaFaqs);

            // Salva as mudanças no banco de dados
            await _context.SaveChangesAsync();
        }


    }
}