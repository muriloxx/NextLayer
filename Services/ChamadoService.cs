// --- ARQUIVO: Services/ChamadoService.cs (COMPLETO E COMENTADO) ---

// Usings necessários para Entity Framework, Models, ViewModels, Injeção de Dependência, etc.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Para logs
using NextLayer.Data; // Onde está o AppDbContext
using NextLayer.Models; // Onde estão as entidades (Chamado, MensagemChat, Anexo, Client, Employee)
using NextLayer.ViewModels; // Onde estão os DTOs (DetalheChamadoViewModel, etc.)
using shortid; // Para gerar IDs curtos para NumeroChamado
using System;
using System.Collections.Generic;
using System.IO; // Usado indiretamente (embora não diretamente aqui)
using System.Linq;
using System.Threading.Tasks;

namespace NextLayer.Services
{
    /// <summary>
    /// Serviço responsável pela lógica de negócios relacionada aos chamados de suporte técnico.
    /// Inclui criação, busca, atualização de chamados e gerenciamento de mensagens do chat.
    /// </summary>
    public class ChamadoService : IChamadoService
    {
        // Dependência do Contexto do Banco de Dados para acessar as tabelas
        private readonly AppDbContext _context;
        // Dependência do Serviço de Armazenamento de Arquivos para upload de anexos
        private readonly IFileStorageService _fileStorage;
        // Dependência do Serviço de IA para gerar respostas no chat
        private readonly IIaService _iaService; // Interface original (implementada por GroqIaService)
        // Dependência do Logger para registrar informações e erros
        private readonly ILogger<ChamadoService> _logger;

        /// <summary>
        /// Construtor que recebe as dependências via Injeção de Dependência.
        /// </summary>
        public ChamadoService(AppDbContext context,
                              IFileStorageService fileStorage,
                              IIaService iaService, // Recebe a implementação da IA (GroqIaService)
                              ILogger<ChamadoService> logger)
        {
            _context = context;
            _fileStorage = fileStorage;
            _iaService = iaService;
            _logger = logger;
        }

        /// <summary>
        /// Cria um novo chamado no sistema, incluindo anexos e a primeira resposta da IA.
        /// </summary>
        /// <param name="model">ViewModel com os dados do novo chamado (título, descrição, imagens).</param>
        /// <param name="clienteId">ID do cliente que está abrindo o chamado.</param>
        /// <returns>ViewModel com os detalhes do chamado recém-criado.</returns>
        /// <exception cref="KeyNotFoundException">Lançada se o clienteId não for encontrado.</exception>
        public async Task<DetalheChamadoViewModel> CriarNovoChamado(CriarChamadoViewModel model, int clienteId)
        {
            // Busca o cliente no banco para garantir que ele existe e pegar o nome
            var cliente = await _context.Clients.FindAsync(clienteId);
            if (cliente == null)
            {
                _logger.LogError("ClienteId {ClienteId} inexistente ao tentar criar chamado.", clienteId);
                throw new KeyNotFoundException("Cliente não encontrado."); // Informa ao Controller sobre o erro
            }
            _logger.LogInformation("Iniciando criação de chamado para ClienteId {ClienteId}", clienteId);

            // Cria a entidade Chamado com os dados básicos
            var novoChamado = new Chamado
            {
                NumeroChamado = $"HD-{ShortId.Generate().ToUpper()}", // Gera um ID curto e único
                Titulo = model.Titulo,
                Descricao = model.Descricao,
                DataAbertura = DateTime.UtcNow, // Usa UTC para datas no servidor
                Status = "Aberto (IA)", // Status inicial indicando que a IA vai responder
                Prioridade = "Média", // Prioridade padrão
                ClienteId = clienteId,
                AnalistaInteragiu = false // Analista ainda não interagiu
                // As coleções Anexos e Mensagens são inicializadas no construtor de Chamado
            };

            // Processa e salva os arquivos anexados (imagens)
            if (model.Imagens != null && model.Imagens.Count > 0)
            {
                _logger.LogInformation("Processando {Count} anexos para o novo chamado {NumeroChamado}", model.Imagens.Count, novoChamado.NumeroChamado);
                foreach (var imagem in model.Imagens)
                {
                    try
                    {
                        // Define um subdiretório específico para este chamado para organizar os uploads
                        var diretorio = $"uploads/chamados/{novoChamado.NumeroChamado}";
                        // Chama o serviço de armazenamento para salvar o arquivo e obter a URL pública
                        var urlArquivo = await _fileStorage.SalvarArquivo(imagem, diretorio);
                        // Cria a entidade Anexo e adiciona à coleção do chamado
                        novoChamado.Anexos.Add(new Anexo
                        {
                            NomeArquivo = imagem.FileName,
                            UrlArquivo = urlArquivo,
                            TipoConteudo = imagem.ContentType,
                            DataUpload = DateTime.UtcNow
                        });
                    }
                    catch (Exception ex)
                    {
                        // Loga o erro mas continua o processo (não impede a criação do chamado por falha no anexo)
                        _logger.LogError(ex, "Erro ao salvar anexo {FileName} para o chamado {NumeroChamado}", imagem.FileName, novoChamado.NumeroChamado);
                    }
                }
            }

            // Adiciona a primeira mensagem ao chat: a descrição do problema enviada pelo cliente
            novoChamado.Mensagens.Add(new MensagemChat
            {
                Conteudo = $"Problema inicial: {model.Descricao}",
                DataEnvio = DateTime.UtcNow,
                ClienteRemetenteId = clienteId,
                RemetenteNome = cliente.Name // Salva o nome do cliente na mensagem
            });

            // Adiciona a segunda mensagem: a saudação padrão da IA
            novoChamado.Mensagens.Add(new MensagemChat
            {
                Conteudo = "Ola eu sou a IA da NextLayer e irei iniciar o seu atendimento e caso seja necessário a intervenção de um analista no futuro, iremos direciona-lo",
                DataEnvio = DateTime.UtcNow.AddSeconds(1), // Um segundo depois da do cliente
                RemetenteNome = "IA NextLayer"
            });

            // Adiciona a terceira mensagem: a resposta da IA ao problema descrito
            _logger.LogInformation("Gerando 1ª resposta contextual da IA para {NumeroChamado}", novoChamado.NumeroChamado);
            string respostaContextualIa = "(Ocorreu um erro interno ao processar a resposta da IA.)"; // Mensagem de fallback
            try
            {
                // Chama o serviço de IA injetado (GroqIaService), passando o chamado (com as 2 msgs anteriores) e a descrição
                respostaContextualIa = await _iaService.GerarRespostaAsync(novoChamado, model.Descricao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar a 1ª resposta contextual da IA para {NumeroChamado}", novoChamado.NumeroChamado);
                novoChamado.Status = "Aguardando Analista"; // Se a IA falhar, já encaminha
            }
            // Adiciona a resposta da IA (ou a mensagem de erro) ao chat
            novoChamado.Mensagens.Add(new MensagemChat
            {
                Conteudo = respostaContextualIa,
                DataEnvio = DateTime.UtcNow.AddSeconds(2), // Um segundo depois da saudação
                RemetenteNome = "IA NextLayer"
            });

            // Adiciona a entidade Chamado (com seus Anexos e Mensagens aninhados) ao DbContext
            _context.Chamados.Add(novoChamado);
            // Salva todas as alterações no banco de dados (INSERTs nas tabelas Chamados, MensagensChat, Anexos)
            await _context.SaveChangesAsync();
            _logger.LogInformation("Chamado {NumeroChamado} (ID {ChamadoId}) criado com sucesso.", novoChamado.NumeroChamado, novoChamado.Id);

            // Mapeia a entidade Chamado recém-criada para o ViewModel de Detalhe e retorna
            return MapearParaDetalheViewModel(novoChamado, cliente.Name);
        }

        /// <summary>
        /// Adiciona uma nova mensagem a um chamado existente e, se aplicável, gera uma resposta da IA.
        /// </summary>
        /// <param name="chamadoId">ID do chamado onde a mensagem será adicionada.</param>
        /// <param name="conteudo">Texto da mensagem.</param>
        /// <param name="remetenteId">ID do Cliente ou Funcionário que enviou a mensagem.</param>
        /// <param name="tipoRemetente">"Client" ou "Employee".</param>
        /// <returns>Lista atualizada de MensagemViewModel para o chat.</returns>
        /// <exception cref="KeyNotFoundException">Lançada se o chamadoId não for encontrado.</exception>
        public async Task<List<MensagemViewModel>> AdicionarMensagem(int chamadoId, string conteudo, int remetenteId, string tipoRemetente)
        {
            _logger.LogInformation("Adicionando mensagem ao ChamadoId {ChamadoId} por {TipoRemetente} ID {RemetenteId}", chamadoId, tipoRemetente, remetenteId);

            // Busca o chamado no banco, incluindo as coleções e entidades relacionadas necessárias
            var chamado = await _context.Chamados
                                .Include(c => c.Mensagens) // Inclui o histórico de chat para a IA e para retorno
                                .Include(c => c.Cliente) // Inclui o cliente para pegar o nome
                                .Include(c => c.Analista) // Inclui o analista para pegar o nome
                                .FirstOrDefaultAsync(c => c.Id == chamadoId);

            if (chamado == null)
            {
                _logger.LogWarning("Tentativa de adicionar mensagem a ChamadoId {ChamadoId} inexistente.", chamadoId);
                throw new KeyNotFoundException("Chamado não encontrado.");
            }

            string nomeCliente = chamado.Cliente?.Name ?? "Cliente Desconhecido";
            string nomeRemetente = nomeCliente; // Assume cliente como padrão
            bool deveChamarIa = false; // Flag para controlar se a IA deve responder

            // 1. Cria a entidade MensagemChat com os dados recebidos
            var novaMensagem = new MensagemChat
            {
                ChamadoId = chamadoId,
                Conteudo = conteudo,
                DataEnvio = DateTime.UtcNow
            };

            // Configura os dados do remetente e a lógica de interação
            if (tipoRemetente == "Client")
            {
                novaMensagem.ClienteRemetenteId = remetenteId;
                novaMensagem.RemetenteNome = nomeCliente;
                // IA só deve responder se um analista AINDA NÃO interagiu neste chamado
                if (!chamado.AnalistaInteragiu)
                {
                    _logger.LogInformation("IA responderá ao cliente no ChamadoId {ChamadoId} (analista não interagiu)", chamadoId);
                    deveChamarIa = true;
                }
                else
                {
                    _logger.LogInformation("IA NÃO responderá ao cliente no ChamadoId {ChamadoId} (analista já interagiu)", chamadoId);
                }
            }
            else if (tipoRemetente == "Employee")
            {
                // Busca o nome do funcionário (analista)
                var analista = await _context.Employees.FindAsync(remetenteId);
                nomeRemetente = analista?.Name ?? "Analista Desconhecido";
                novaMensagem.FuncionarioRemetenteId = remetenteId;
                novaMensagem.RemetenteNome = nomeRemetente;

                // --- Lógica de Intervenção do Analista ---
                // Marca que um analista interagiu (se for a primeira vez)
                if (!chamado.AnalistaInteragiu)
                {
                    _logger.LogInformation("Primeira interação do Analista ID {AnalistaId} no ChamadoId {ChamadoId}. Desativando IA para este chamado.", remetenteId, chamadoId);
                    chamado.AnalistaInteragiu = true;
                }
                // Muda o status do chamado se ele estava sendo tratado pela IA ou aguardando
                if (chamado.Status.Contains("IA") || chamado.Status.Contains("Aguardando"))
                {
                    chamado.Status = "Em Andamento (Analista)";
                }
                // Atribui (ou reatribui) o chamado ao analista que respondeu
                if (!chamado.AnalistaId.HasValue || chamado.AnalistaId != remetenteId)
                {
                    chamado.AnalistaId = remetenteId;
                    _logger.LogInformation("ChamadoId {ChamadoId} atribuído/reatribuído ao Analista ID {AnalistaId}", chamadoId, remetenteId);
                }
                // --- FIM DA LÓGICA ---
            }
            else
            {
                _logger.LogWarning("TipoRemetente desconhecido '{TipoRemetente}' ao adicionar mensagem para ChamadoId {ChamadoId}", tipoRemetente, chamadoId);
                // Decide como tratar: não salvar, salvar como sistema, etc. Aqui vamos apenas logar.
                novaMensagem.RemetenteNome = "Sistema (Desconhecido)";
            }

            // Adiciona a nova mensagem à coleção do chamado
            chamado.Mensagens.Add(novaMensagem);
            // Salva a nova mensagem E as possíveis alterações no Status, AnalistaId, AnalistaInteragiu do Chamado
            await _context.SaveChangesAsync();
            _logger.LogInformation("Mensagem salva para ChamadoId {ChamadoId}", chamadoId);

            // 2. Chama a IA (SOMENTE se a flag deveChamarIa for true)
            if (deveChamarIa)
            {
                _logger.LogInformation("Gerando resposta da IA para ChamadoId {ChamadoId}", chamadoId);
                string respostaIa = "(Ocorreu um erro ao contatar a IA)"; // Fallback
                try
                {
                    // Chama o serviço de IA injetado, passando o chamado atualizado e a última mensagem do cliente
                    respostaIa = await _iaService.GerarRespostaAsync(chamado, conteudo);

                    // Salva a resposta da IA como uma nova mensagem
                    chamado.Mensagens.Add(new MensagemChat
                    {
                        ChamadoId = chamadoId,
                        Conteudo = respostaIa,
                        DataEnvio = DateTime.UtcNow.AddSeconds(1), // Pequeno delay
                        RemetenteNome = "IA NextLayer"
                    });

                    // Se a IA decidiu encaminhar (baseado no prompt), atualiza o status
                    if (respostaIa.Contains("encaminhando seu chamado"))
                    {
                        _logger.LogInformation("IA encaminhou ChamadoId {ChamadoId} para analista.", chamadoId);
                        chamado.Status = "Aguardando Analista";
                    }

                    await _context.SaveChangesAsync(); // Salva a mensagem da IA e possível mudança de status
                    _logger.LogInformation("Resposta da IA salva para ChamadoId {ChamadoId}", chamadoId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao gerar ou salvar resposta da IA para ChamadoId {ChamadoId}", chamadoId);
                    // Adiciona uma mensagem de erro no chat para o usuário saber que a IA falhou
                    chamado.Mensagens.Add(new MensagemChat
                    {
                        ChamadoId = chamadoId,
                        Conteudo = "(Ocorreu um erro ao processar a resposta da IA. Um analista verá sua mensagem.)",
                        DataEnvio = DateTime.UtcNow.AddSeconds(1),
                        RemetenteNome = "Sistema"
                    });
                    chamado.Status = "Aguardando Analista"; // Garante encaminhamento se IA falhar
                    await _context.SaveChangesAsync();
                }
            }

            // 3. Recarrega as mensagens do banco e retorna a lista atualizada mapeada para ViewModel
            _logger.LogDebug("Recarregando mensagens para ChamadoId {ChamadoId} antes de retornar", chamadoId);
            // É importante recarregar para garantir que o ViewModel retornado contenha todas as mensagens,
            // incluindo a da IA que pode ter sido adicionada.
            await _context.Entry(chamado).Collection(c => c.Mensagens).LoadAsync();
            // Recarrega o analista para garantir que o nome está atualizado no mapeamento
            if (chamado.AnalistaId.HasValue) await _context.Entry(chamado).Reference(c => c.Analista).LoadAsync();
            string nomeAnalista = chamado.Analista?.Name ?? "Analista";

            // Mapeia a lista de MensagemChat para MensagemViewModel
            return chamado.Mensagens.OrderBy(m => m.DataEnvio).Select(m => new MensagemViewModel
            {
                Id = m.Id,
                Conteudo = m.Conteudo ?? "",
                DataEnvio = m.DataEnvio,
                RemetenteNome = m.RemetenteNome ?? // Tenta o nome salvo na mensagem
                                (m.ClienteRemetenteId.HasValue ? nomeCliente :
                                (m.FuncionarioRemetenteId.HasValue ? nomeAnalista : // Usa nome do analista
                                "IA NextLayer")), // Default IA
                TipoRemetente = m.ClienteRemetenteId.HasValue ? "Client" : (m.FuncionarioRemetenteId.HasValue ? "Employee" : "IA")
            }).ToList();
        }

        /// <summary>
        /// Obtém a lista de chamados não fechados para exibição no grid do analista.
        /// </summary>
        /// <returns>Lista de ChamadoGridViewModel.</returns>
        public async Task<IEnumerable<ChamadoGridViewModel>> GetChamadosEmAberto()
        {
            _logger.LogInformation("Buscando chamados não fechados para o grid do analista.");
            return await _context.Chamados
                .Include(c => c.Cliente) // Inclui o Cliente para pegar o nome
                .Where(c => c.Status != "Fechado") // Filtra apenas os não fechados
                .OrderByDescending(c => c.Prioridade == "Alta" ? 3 : (c.Prioridade == "Média" ? 2 : 1)) // Ordena por Prioridade
                .ThenBy(c => c.DataAbertura) // Depois por Data de Abertura (mais antigo primeiro)
                .Select(c => new ChamadoGridViewModel // Mapeia para o ViewModel do Grid
                {
                    Id = c.Id,
                    NumeroChamado = c.NumeroChamado ?? "N/A",
                    Titulo = c.Titulo ?? "Sem Título",
                    NomeCliente = c.Cliente.Name ?? "Cliente Desconhecido",
                    DataAbertura = c.DataAbertura,
                    Status = c.Status ?? "N/A"
                })
                .ToListAsync(); // Executa a consulta no banco
        }

        /// <summary>
        /// Obtém a lista de chamados de um cliente específico.
        /// </summary>
        /// <param name="clienteId">ID do cliente.</param>
        /// <returns>Lista de ChamadoGridViewModel.</returns>
        public async Task<IEnumerable<ChamadoGridViewModel>> GetChamadosPorCliente(int clienteId)
        {
            _logger.LogInformation("Buscando chamados para ClienteId {ClienteId}", clienteId);
            return await _context.Chamados
                .Where(c => c.ClienteId == clienteId) // Filtra pelo ID do cliente
                .OrderByDescending(c => c.DataAbertura) // Ordena por data (mais recente primeiro)
                .Select(c => new ChamadoGridViewModel // Mapeia para o ViewModel do Grid
                {
                    Id = c.Id,
                    NumeroChamado = c.NumeroChamado ?? "N/A",
                    Titulo = c.Titulo ?? "Sem Título",
                    NomeCliente = null, // Não precisa repetir o nome do cliente para ele mesmo
                    DataAbertura = c.DataAbertura,
                    Status = c.Status ?? "N/A"
                })
                .ToListAsync();
        }

        /// <summary>
        /// Obtém os detalhes completos de um chamado, incluindo mensagens e anexos, para a tela de chat.
        /// </summary>
        /// <param name="chamadoId">ID do chamado.</param>
        /// <returns>ViewModel DetalheChamadoViewModel ou null se não encontrado.</returns>
        public async Task<DetalheChamadoViewModel> GetDetalheChamado(int chamadoId)
        {
            _logger.LogInformation("Buscando detalhes completos para ChamadoId {ChamadoId}", chamadoId);
            var chamado = await _context.Chamados
                .Include(c => c.Cliente) // Inclui Cliente
                .Include(c => c.Analista) // Inclui Analista
                .Include(c => c.Mensagens.OrderBy(m => m.DataEnvio)) // Inclui Mensagens ORDENADAS
                .Include(c => c.Anexos) // Inclui Anexos
                .FirstOrDefaultAsync(c => c.Id == chamadoId); // Busca pelo ID

            if (chamado == null)
            {
                _logger.LogWarning("ChamadoId {ChamadoId} não encontrado ao buscar detalhes.", chamadoId);
                return null!; // Retorna null (Controller tratará como 404 Not Found)
            }

            // Usa o método helper para mapear a entidade para o ViewModel
            var nomeCliente = chamado.Cliente?.Name ?? "Cliente Desconhecido";
            return MapearParaDetalheViewModel(chamado, nomeCliente);
        }

        /// <summary>
        /// Atualiza os campos de status, prioridade, role designada e analista de um chamado.
        /// Usado pela tela do analista.
        /// </summary>
        /// <param name="chamadoId">ID do chamado a ser atualizado.</param>
        /// <param name="model">ViewModel com os novos dados.</param>
        /// <returns>A entidade Chamado atualizada.</returns>
        /// <exception cref="KeyNotFoundException">Lançada se o chamadoId não for encontrado.</exception>
        public async Task<Chamado> AtualizarChamado(int chamadoId, AtualizarChamadoViewModel model)
        {
            _logger.LogInformation("Atualizando ChamadoId {ChamadoId} com Status={Status}, Prioridade={Prioridade}, AnalistaId={AnalistaId}",
               chamadoId, model.Status, model.Prioridade, model.AnalistaId);

            var chamado = await _context.Chamados.FindAsync(chamadoId);
            if (chamado == null)
            {
                _logger.LogWarning("Tentativa de atualizar ChamadoId {ChamadoId} inexistente.", chamadoId);
                throw new KeyNotFoundException("Chamado não encontrado.");
            }

            // Atualiza as propriedades da entidade com os valores do ViewModel
            chamado.Status = model.Status;
            chamado.Prioridade = model.Prioridade;
            chamado.RoleDesignada = model.RoleDesignada; // Pode ser null
            chamado.AnalistaId = model.AnalistaId; // Pode ser null

            // Regra de negócio: Se um analista for explicitamente atribuído,
            // marca como interagido para desativar a IA (caso ainda não esteja).
            if (model.AnalistaId.HasValue && !chamado.AnalistaInteragiu)
            {
                _logger.LogInformation("Marcando ChamadoId {ChamadoId} como interagido devido à atribuição manual de analista.", chamadoId);
                chamado.AnalistaInteragiu = true;
                // Opcional: Mudar status automaticamente para "Em Andamento" ao atribuir?
                // if (chamado.Status.Contains("Aberto") || chamado.Status.Contains("Aguardando")) {
                //     chamado.Status = "Em Andamento (Analista)";
                // }
            }

            _context.Chamados.Update(chamado); // Marca a entidade como modificada
            await _context.SaveChangesAsync(); // Salva as alterações no banco
            _logger.LogInformation("ChamadoId {ChamadoId} atualizado com sucesso.", chamadoId);
            return chamado; // Retorna a entidade atualizada
        }

        /// <summary>
        /// Método auxiliar privado para mapear uma entidade Chamado (com suas coleções carregadas)
        /// para o ViewModel DetalheChamadoViewModel, que é seguro para serialização JSON (sem loops).
        /// </summary>
        /// <param name="chamado">A entidade Chamado carregada do banco.</param>
        /// <param name="nomeCliente">O nome do cliente associado.</param>
        /// <returns>O ViewModel DetalheChamadoViewModel preenchido.</returns>
        private DetalheChamadoViewModel MapearParaDetalheViewModel(Chamado chamado, string nomeCliente)
        {
            // Pega o nome do analista (se houver) para usar no mapeamento das mensagens
            string nomeAnalista = chamado.Analista?.Name ?? "Analista";

            // Cria e retorna o ViewModel
            return new DetalheChamadoViewModel
            {
                Id = chamado.Id,
                NumeroChamado = chamado.NumeroChamado ?? "N/A",
                Titulo = chamado.Titulo ?? "Sem Título",
                Descricao = chamado.Descricao ?? "Sem Descrição",
                DataAbertura = chamado.DataAbertura,
                Status = chamado.Status ?? "N/A",
                NomeCliente = nomeCliente,

                // Mapeia a coleção de Anexos para AnexoViewModel
                // Usa ?. para segurança caso a coleção seja nula (embora o construtor inicialize)
                // Usa ?? new List... para garantir que sempre retorne uma lista, nunca null
                Anexos = chamado.Anexos?.Select(a => new AnexoViewModel
                {
                    Id = a.Id,
                    NomeArquivo = a.NomeArquivo ?? "arquivo_sem_nome",
                    UrlArquivo = a.UrlArquivo ?? "#" // Retorna '#' se a URL for nula/vazia
                }).ToList() ?? new List<AnexoViewModel>(),

                // Mapeia a coleção de Mensagens (já ordenada) para MensagemViewModel
                Mensagens = chamado.Mensagens?.Select(m => new MensagemViewModel
                {
                    Id = m.Id,
                    Conteudo = m.Conteudo ?? "", // Retorna string vazia se Conteudo for null
                    DataEnvio = m.DataEnvio,
                    // Define o nome do remetente com base nos IDs ou no nome salvo
                    RemetenteNome = m.RemetenteNome ?? // Prefere o nome salvo na mensagem
                                    (m.ClienteRemetenteId.HasValue ? nomeCliente :
                                    (m.FuncionarioRemetenteId.HasValue ? nomeAnalista : // Usa o nome carregado do analista
                                    "IA NextLayer")), // Default para IA
                    // Define o tipo do remetente para o front-end usar no CSS
                    TipoRemetente = m.ClienteRemetenteId.HasValue ? "Client" : (m.FuncionarioRemetenteId.HasValue ? "Employee" : "IA")
                }).ToList() ?? new List<MensagemViewModel>()
            };
        }
    }
}