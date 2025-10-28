using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Para logs
using NextLayer.Data;
using NextLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLayer.Services
{
    /// <summary>
    /// Serviço para gerenciar os itens da Base de Conhecimento (FAQ).
    /// </summary>
    public class FaqService : IFaqService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FaqService> _logger;
        // Futuramente, pode injetar o IChatIaService aqui para sugestões via IA
        // private readonly IChatIaService _iaService;

        public FaqService(AppDbContext context, ILogger<FaqService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os itens de FAQ ordenados por ID (ou outro critério).
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
                return new List<FaqItem>(); // Retorna lista vazia em caso de erro
            }
        }

        /// <summary>
        /// Sugere FAQs buscando por palavras-chave no título e descrição.
        /// Implementação simples inicial.
        /// </summary>
        public async Task<List<FaqItem>> GetFaqSugestoesAsync(string titulo, string descricao)
        {
            _logger.LogInformation("Buscando sugestões de FAQ para: Titulo='{Titulo}', Descricao='{Descricao}'", titulo, descricao);

            if (string.IsNullOrWhiteSpace(titulo) && string.IsNullOrWhiteSpace(descricao))
            {
                return new List<FaqItem>(); // Retorna vazio se não houver texto
            }

            // Combina título e descrição e divide em palavras-chave (simples)
            var textoBusca = $"{titulo} {descricao}".ToLowerInvariant();
            var palavrasChave = textoBusca.Split(new[] { ' ', ',', '.', ';', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Distinct()
                                          .Where(p => p.Length > 2) // Ignora palavras muito curtas
                                          .ToList();

            if (!palavrasChave.Any())
            {
                return new List<FaqItem>();
            }

            try
            {
                // Busca no banco por FAQs que contenham QUALQUER uma das palavras-chave
                // na Pergunta OU na Resposta (ToLower para busca case-insensitive)
                // ATENÇÃO: Esta busca pode ser lenta em bancos grandes. Otimizações (Full-Text Search) seriam necessárias.
                var sugestoes = await _context.FaqItems
                    .Where(f => palavrasChave.Any(p => f.Pergunta.ToLower().Contains(p) || f.Resposta.ToLower().Contains(p)))
                    .Take(5) // Limita a 5 sugestões
                    .ToListAsync();

                _logger.LogInformation("Encontradas {Count} sugestões de FAQ.", sugestoes.Count);
                return sugestoes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar sugestões de FAQ para: Titulo='{Titulo}', Descricao='{Descricao}'", titulo, descricao);
                return new List<FaqItem>(); // Retorna lista vazia em caso de erro
            }

            // --- Implementação Futura com IA ---
            // var todasFaqs = await GetAllFaqsAsync();
            // var idsSugeridos = await _iaService.SugerirFaqIdsAsync(titulo, descricao, todasFaqs);
            // return todasFaqs.Where(f => idsSugeridos.Contains(f.Id)).ToList();
            // --- Fim da Implementação Futura ---
        }

        // --- Método para Popular FAQs (USAR APENAS UMA VEZ ou em SEED) ---
        /// <summary>
        /// Adiciona os exemplos de FAQ iniciais ao banco de dados, se ele estiver vazio.
        /// </summary>
        public async Task SeedInitialFaqsAsync()
        {
            if (!await _context.FaqItems.AnyAsync()) // Verifica se a tabela está vazia
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
            else
            {
                _logger.LogInformation("Tabela FaqItens já contém dados. Seed não executado.");
            }
        }
    }
}