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
        private readonly ILogger<FaqService> _logger;
        private readonly IIaService _iaService;

        public FaqService(AppDbContext context,
                                  ILogger<FaqService> logger,
                                  IIaService iaService)
        {
            _context = context;
            _logger = logger;
            _iaService = iaService;
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
        /// Adiciona os exemplos de FAQ iniciais ao banco de dados, se ele estiver vazio.
        /// Este método é chamado uma vez na inicialização (em Program.cs).
        /// </summary>
        public async Task SeedInitialFaqsAsync()
        {
            // Verifica se a tabela FaqItens já tem algum registro
            if (!await _context.FaqItens.AnyAsync())
            {
                _logger.LogInformation("Populando tabela FaqItens com dados iniciais...");

                var faqsIniciais = new List<FaqItem>
                {
                    // FAQ 1 (Existente)
                    new FaqItem {
                        Pergunta = "Como resetar minha senha de rede/sistema?",
                        Resposta = "Para resetar sua senha, acesse o portal interno em [link_do_portal_senha] e clique em \"Esqueci minha senha\". Siga as instruções enviadas para o seu e-mail de recuperação. Se não tiver acesso ao e-mail, abra um chamado detalhando o problema.",
                        DataCriacao = DateTime.UtcNow
                    },
                    // FAQ 2 (Existente)
                    new FaqItem {
                        Pergunta = "A impressora não está funcionando. O que fazer?",
                        Resposta = "Verifique se a impressora está ligada e conectada à rede (cabo ou Wi-Fi). Certifique-se de que há papel na bandeja e que não há mensagens de erro no painel. Tente reiniciar a impressora e seu computador. Se o problema persistir, anote o modelo da impressora e o código de erro (se houver) e abra um chamado.",
                        DataCriacao = DateTime.UtcNow
                    },
                    // FAQ 3 (Existente)
                    new FaqItem {
                        Pergunta = "Não consigo acessar a pasta compartilhada da rede.",
                        Resposta = "Verifique sua conexão de rede (cabo ou Wi-Fi). Tente acessar outras pastas ou recursos da rede para confirmar a conexão. Se você consegue acessar outros recursos, pode ser um problema de permissão. Anote o caminho completo da pasta (ex: \\\\servidor\\departamento) e abra um chamado solicitando a verificação de acesso.",
                        DataCriacao = DateTime.UtcNow
                    },
                    // FAQ 4 (Existente)
                    new FaqItem {
                        Pergunta = "Meu computador está muito lento.",
                        Resposta = "Feche todos os programas que não estiverem utilizando. Reinicie o computador. Verifique se há atualizações pendentes do Windows ou de outros softwares. Execute uma verificação de vírus com o antivírus corporativo. Se a lentidão continuar, abra um chamado informando desde quando o problema ocorre e quais programas parecem ser mais afetados.",
                        DataCriacao = DateTime.UtcNow
                    },
                    // FAQ 5 (Existente)
                    new FaqItem {
                        Pergunta = "Como configurar o e-mail no meu celular?",
                        Resposta = "Siga o guia passo-a-passo disponível na Intranet em [link_do_guia_email_mobile]. Você precisará do seu e-mail corporativo, senha e, possivelmente, das configurações do servidor (IMAP/SMTP) descritas no guia. Se encontrar dificuldades, abra um chamado informando o modelo do seu celular e o passo onde ocorreu o erro.",
                        DataCriacao = DateTime.UtcNow
                    },
                    // --- NOVOS FAQs ---
                    // FAQ 6
                    new FaqItem {
                        Pergunta = "Minha conta de rede está bloqueada. O que eu faço?",
                        Resposta = "O bloqueio de conta é automático por 30 minutos após 5 tentativas incorretas. Por favor, aguarde 30 minutos e tente novamente com cuidado. Se a urgência for alta, abra um chamado solicitando o desbloqueio manual.",
                        DataCriacao = DateTime.UtcNow
                    },
                    // FAQ 7
                    new FaqItem {
                        Pergunta = "Não consigo me conectar à VPN para trabalhar de casa.",
                        Resposta = "1. Verifique se sua conexão de internet local está funcionando. 2. Abra o software de VPN (ex: Cisco AnyConnect) e verifique se o endereço do servidor (vpn.nextlayer.com) está correto. 3. Reinicie o software de VPN e tente conectar novamente. 4. Siga o guia de configuração da VPN disponível na Intranet [link_do_guia_vpn].",
                        DataCriacao = DateTime.UtcNow
                    },
                    // FAQ 8
                    new FaqItem {
                        Pergunta = "O Microsoft Excel (ou Word/Teams) está travando.",
                        Resposta = "1. Salve seu trabalho e feche o programa. 2. Tente um \"Reparo Rápido\" do Office: Vá em Painel de Controle > Programas e Recursos > Microsoft Office > Alterar > Reparo Rápido. 3. Se o problema persistir, abra um chamado informando qual arquivo específico está travando.",
                        DataCriacao = DateTime.UtcNow
                    },
                    // FAQ 9
                    new FaqItem {
                        Pergunta = "Como posso solicitar a instalação de um novo software?",
                        Resposta = "A instalação de softwares não-padrão (ex: Adobe Photoshop, Power BI) requer aprovação do seu gestor. Por favor, abra um chamado na categoria 'Solicitação de Software', inclua a justificativa de negócio (por que você precisa do software) e o e-mail do seu gestor para aprovação.",
                        DataCriacao = DateTime.UtcNow
                    },
                    // FAQ 10
                    new FaqItem {
                        Pergunta = "Meu mouse (ou teclado) sem fio parou de funcionar.",
                        Resposta = "1. Tente trocar as pilhas/baterias. 2. Desconecte o adaptador USB (dongle) e conecte-o em outra porta USB. 3. Reinicie o computador. 4. Se nada disso funcionar, abra um chamado na categoria 'Hardware' solicitando a substituição do periférico.",
                        DataCriacao = DateTime.UtcNow
                    }
                };

                // Adiciona todos os 10 ao banco de dados
                await _context.FaqItens.AddRangeAsync(faqsIniciais);
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