// --- ARQUIVO: Services/FaqService.cs (COMPLETO E ATUALIZADO) ---
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NextLayer.Data;
using NextLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLayer.Services
{
    public class FaqService : IFaqService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FaqService> _logger;
        // --- INJETAR A INTERFACE DA IA COM O NOME CORRETO ---
        private readonly IIaService _iaService;

        // --- CONSTRUTOR ATUALIZADO ---
        public FaqService(AppDbContext context,
                          ILogger<FaqService> logger,
                          IIaService iaService) // <-- Adicionado IIaService
        {
            _context = context;
            _logger = logger;
            _iaService = iaService; // <-- Adicionado
        }

        /// <summary>
        /// Obtém todos os itens de FAQ ordenados por ID.
        /// </summary>
        public async Task<List<FaqItem>> GetAllFaqsAsync()
        {
            _logger.LogInformation("Buscando todos os itens de FAQ.");
            try
            {
                return await _context.FaqItems.OrderBy(f => f.Id).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os itens de FAQ.");
                return new List<FaqItem>();
            }
        }

        // --- MÉTODO ATUALIZADO PARA USAR A IA ---
        /// <summary>
        /// Sugere FAQs usando a IA para comparar o problema com a base de conhecimento.
        /// </summary>
        public async Task<List<FaqItem>> GetFaqSugestoesAsync(string titulo, string descricao)
        {
            _logger.LogInformation("Buscando sugestões de FAQ (via IA) para: {Titulo}", titulo);

            if (string.IsNullOrWhiteSpace(titulo) && string.IsNullOrWhiteSpace(descricao))
            {
                return new List<FaqItem>();
            }

            try
            {
                // 1. Busca todas as FAQs do banco (para a IA analisar)
                var todasFaqs = await GetAllFaqsAsync();
                if (!todasFaqs.Any())
                {
                    _logger.LogWarning("Nenhum FAQ encontrado no banco para sugestão.");
                    return new List<FaqItem>();
                }

                // 2. Chama a IA (usando a interface IIaService)
                var idsSugeridos = await _iaService.SugerirFaqsRelevantesAsync(titulo, descricao, todasFaqs);

                if (!idsSugeridos.Any())
                {
                    _logger.LogInformation("IA não sugeriu FAQs relevantes.");
                    return new List<FaqItem>();
                }

                // 3. Filtra a lista original para retornar apenas os itens sugeridos
                var sugestoes = todasFaqs
                    .Where(f => idsSugeridos.Contains(f.Id))
                    .ToList();

                _logger.LogInformation("Encontradas {Count} sugestões de FAQ via IA.", sugestoes.Count);
                return sugestoes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar sugestões de FAQ (IA) para: {Titulo}", titulo);
                return new List<FaqItem>();
            }
        }
        // --- FIM DA ATUALIZAÇÃO ---


        /// <summary>
        /// Adiciona os exemplos de FAQ iniciais ao banco de dados, se ele estiver vazio.
        /// </summary>
        public async Task SeedInitialFaqsAsync()
        {
            if (!await _context.FaqItems.AnyAsync())
            {
                _logger.LogInformation("Populando tabela FaqItens com dados iniciais...");
                var faqsIniciais = new List<FaqItem>
                {
                    new FaqItem { Pergunta = "Como resetar minha senha de rede/sistema?", Resposta = "Para resetar sua senha, acesse o portal interno em [link_do_portal] e clique em \"Esqueci minha senha\". Siga as instruções enviadas para o seu e-mail de recuperação. Se não tiver acesso ao e-mail, abra um chamado detalhando o problema.", DataCriacao = DateTime.UtcNow },
                    new FaqItem { Pergunta = "A impressora não está funcionando. O que fazer?", Resposta = "Verifique se a impressora está ligada e conectada à rede (cabo ou Wi-Fi). Certifique-se de que há papel na bandeja e que não há mensagens de erro no painel. Tente reiniciar a impressora e seu computador. Se o problema persistir, anote o modelo da impressora e o código de erro (se houver) e abra um chamado.", DataCriacao = DateTime.UtcNow },
                    new FaqItem { Pergunta = "Não consigo acessar a pasta compartilhada da rede.", Resposta = "Verifique sua conexão de rede (cabo ou Wi-Fi). Tente acessar outras pastas ou recursos da rede para confirmar a conexão. Se você consegue acessar outros recursos, pode ser um problema de permissão. Anote o caminho completo da pasta (ex: \\\\servidor\\departamento) e abra um chamado solicitando a verificação de acesso.", DataCriacao = DateTime.UtcNow },
                    new FaqItem { Pergunta = "Meu computador está muito lento.", Resposta = "Feche todos os programas que não estiver utilizando. Reinicie o computador. Verifique se há atualizações pendentes do Windows ou de outros softwares. Execute uma verificação de vírus com o antivírus corporativo. Se a lentidão continuar, abra um chamado informando desde quando o problema ocorre e quais programas parecem ser mais afetados.", DataCriacao = DateTime.UtcNow },
                    new FaqItem { Pergunta = "Como configurar o e-mail no meu celular?", Resposta = "Siga o guia passo-a-passo disponível na Intranet em [link_do_guia_email_mobile]. Você precisará do seu e-mail corporativo, senha e, possivelmente, das configurações do servidor (IMAP/SMTP) descritas no guia. Se encontrar dificuldades, abra um chamado informando o modelo do seu celular e o passo onde ocorreu o erro.", DataCriacao = DateTime.UtcNow }
                };
                await _context.FaqItems.AddRangeAsync(faqsIniciais);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Tabela FaqItens populada com {Count} itens.", faqsIniciais.Count);
            }
            else { _logger.LogInformation("Tabela FaqItens já contém dados. Seed não executado."); }
        }
    }
}