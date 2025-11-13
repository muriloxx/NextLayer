using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NextLayer.Data;
using NextLayer.Models;
using NextLayer.ViewModels; // Necessário para IaResposta
using shortid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLayer.Services
{
    public class ChamadoService : IChamadoService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorage;
        private readonly IIaService _iaService; // Interface original
        private readonly ILogger<ChamadoService> _logger;

        public ChamadoService(AppDbContext context,
                              IFileStorageService fileStorage,
                              IIaService iaService, // Interface original
                              ILogger<ChamadoService> logger)
        {
            _context = context;
            _fileStorage = fileStorage;
            _iaService = iaService;
            _logger = logger;
        }

        public async Task<List<ChamadoGridViewModel>> GetAllChamadosAdminAsync(string? status, string? prioridade)
        {
            // Começa com uma query que pega todos os chamados
            var query = _context.Chamados
                                .Include(c => c.Cliente) // <-- Força o carregamento do Cliente
                                .Include(c => c.Analista) // <-- Força o carregamento do Analista
                                .AsQueryable();

            // Aplica o filtro de status, se ele foi fornecido
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.Status == status);
            }

            // Aplica o filtro de prioridade, se ele foi fornecido
            if (!string.IsNullOrEmpty(prioridade))
            {
                query = query.Where(c => c.Prioridade == prioridade);
            }

            // Projeta o resultado para o ViewModel
            var chamados = await query
                .OrderByDescending(c => c.DataAbertura)
                .Select(c => new ChamadoGridViewModel
                {
                    Id = c.Id,
                    NumeroChamado = c.NumeroChamado,
                    Titulo = c.Titulo,
                    Status = c.Status,
                    Prioridade = c.Prioridade,
                    DataAbertura = c.DataAbertura,
                    NomeCliente = c.Cliente.Name, // Esta linha agora DEVE funcionar
                    NomeAnalista = c.Analista != null ? c.Analista.Name : null
                })
                .ToListAsync();

            return chamados;
        }

        public async Task<DetalheChamadoViewModel> CriarNovoChamado(CriarChamadoViewModel model, int clienteId)
        {
            var cliente = await _context.Clients.FindAsync(clienteId);
            if (cliente == null)
            {
                _logger.LogError("ClienteId {Id} inexistente.", clienteId);
                throw new KeyNotFoundException("Cliente não encontrado.");
            }

            var novoChamado = new Chamado
            {
                NumeroChamado = $"HD-{ShortId.Generate().ToUpper()}",
                Titulo = model.Titulo,
                Descricao = model.Descricao,
                DataAbertura = DateTime.UtcNow,
                Status = "Aberto (IA)",
                Prioridade = "Média",
                ClienteId = clienteId,
                AnalistaInteragiu = false
            };

            // --- INÍCIO DA LÓGICA DE UPLOAD ---
            if (model.Imagens != null && model.Imagens.Any())
            {
                _logger.LogInformation("Iniciando upload de {Count} anexos para o chamado {Num}", model.Imagens.Count, novoChamado.NumeroChamado);

                // Define o diretório de destino baseado no número do chamado
                // O LocalFileStorageService salvará isso dentro de wwwroot
                string diretorioDestino = $"uploads/chamados/{novoChamado.NumeroChamado}";

                foreach (var arquivo in model.Imagens)
                {
                    try
                    {
                        // 1. Salva o arquivo no disco (ex: wwwroot/uploads/chamados/HD-XYZ/arquivo.png)
                        //    e obtém a URL pública (ex: https://localhost:7121/uploads/chamados/HD-XYZ/arquivo.png)
                        string urlArquivo = await _fileStorage.SalvarArquivo(arquivo, diretorioDestino);

                        // 2. Cria a entidade Anexo para salvar no banco
                        var novoAnexo = new Anexo
                        {
                            NomeArquivo = arquivo.FileName,
                            UrlArquivo = urlArquivo,
                            TipoConteudo = arquivo.ContentType,
                            DataUpload = DateTime.UtcNow
                            // O EF irá ligar o ChamadoId automaticamente
                        };

                        // 3. Adiciona o anexo à coleção do chamado
                        novoChamado.Anexos.Add(novoAnexo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Falha ao salvar o anexo {NomeArquivo} para o chamado {Num}", arquivo.FileName, novoChamado.NumeroChamado);
                        // Continua o processo mesmo que um anexo falhe
                    }
                }
            }
            // --- FIM DA LÓGICA DE UPLOAD ---

            novoChamado.Mensagens.Add(new MensagemChat { Conteudo = $"Problema inicial: {model.Descricao}", DataEnvio = DateTime.UtcNow, ClienteRemetenteId = clienteId, RemetenteNome = cliente.Name });
            novoChamado.Mensagens.Add(new MensagemChat { Conteudo = "Ola eu sou a IA da NextLayer...", DataEnvio = DateTime.UtcNow.AddSeconds(1), RemetenteNome = "IA NextLayer" });

            IaResposta respostaIaObj = new IaResposta { TextoResposta = "(IA não pôde ser contatada)" };
            try
            {
                respostaIaObj = await _iaService.GerarRespostaAsync(novoChamado, model.Descricao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro 1ª resp IA {Num}", novoChamado.NumeroChamado);
                novoChamado.Status = "Aguardando Analista";
            }
            novoChamado.Mensagens.Add(new MensagemChat { Conteudo = respostaIaObj.TextoResposta, DataEnvio = DateTime.UtcNow.AddSeconds(2), RemetenteNome = "IA NextLayer" });

            // 1. Adiciona o chamado (e seus Anexos e Mensagens) ao context
            _context.Chamados.Add(novoChamado);

            // 2. Salva tudo no banco de dados (Chamado, Anexos, Mensagens)
            await _context.SaveChangesAsync();

            return MapearParaDetalheViewModel(novoChamado, cliente.Name);
        }

        // --- Método AdicionarMensagem (Corrigido para IaResposta e Atribuição) ---
        public async Task<List<MensagemViewModel>> AdicionarMensagem(int chamadoId, string conteudo, int remetenteId, string tipoRemetente)
        {
            var chamado = await _context.Chamados.Include(c => c.Mensagens).Include(c => c.Cliente).Include(c => c.Analista).FirstOrDefaultAsync(c => c.Id == chamadoId);
            if (chamado == null) throw new KeyNotFoundException("Chamado não encontrado.");

            bool isConcluidoHaMaisDe72h = false;
            if (chamado.Status == "Concluído" && chamado.DataConclusao.HasValue && DateTime.UtcNow > chamado.DataConclusao.Value.AddHours(72)) { isConcluidoHaMaisDe72h = true; }
            if (tipoRemetente == "Client" && (chamado.Status == "Encerrado" || isConcluidoHaMaisDe72h)) { throw new InvalidOperationException("Este chamado está encerrado."); }

            string nomeCliente = chamado.Cliente?.Name ?? "Cliente"; string nomeRemetente = nomeCliente; bool deveChamarIa = false;
            var novaMensagem = new MensagemChat { ChamadoId = chamadoId, Conteudo = conteudo, DataEnvio = DateTime.UtcNow };

            if (tipoRemetente == "Client")
            {
                novaMensagem.ClienteRemetenteId = remetenteId; novaMensagem.RemetenteNome = nomeCliente;
                if (!chamado.AnalistaInteragiu) { deveChamarIa = true; }
            }
            else
            { // Employee
                var analista = await _context.Employees.FindAsync(remetenteId); nomeRemetente = analista?.Name ?? "Analista";
                novaMensagem.FuncionarioRemetenteId = remetenteId; novaMensagem.RemetenteNome = nomeRemetente;
                if (!chamado.AnalistaInteragiu) { chamado.AnalistaInteragiu = true; }
                if (chamado.Status.Contains("IA") || chamado.Status.Contains("Aguardando")) { chamado.Status = "Em Andamento (Analista)"; }
                if (!chamado.AnalistaId.HasValue || chamado.AnalistaId != remetenteId) { chamado.AnalistaId = remetenteId; }
            }
            chamado.Mensagens.Add(novaMensagem);
            await _context.SaveChangesAsync();

            if (deveChamarIa)
            {
                IaResposta respostaIaObj = new IaResposta { TextoResposta = "(Erro ao contatar IA)" };
                try
                {
                    respostaIaObj = await _iaService.GerarRespostaAsync(chamado, conteudo);
                    if (respostaIaObj.DeveEncaminhar && !string.IsNullOrEmpty(respostaIaObj.RoleSugerida))
                    {
                        chamado.Status = "Aguardando Analista";
                        var analistaAtribuido = await EncontrarProximoAnalistaPorRoleAsync(respostaIaObj.RoleSugerida);
                        if (analistaAtribuido != null)
                        {
                            chamado.AnalistaId = analistaAtribuido.Id;
                            _logger.LogInformation("Chamado {Id} atribuído auto ao Analista {AId}", chamadoId, analistaAtribuido.Id);
                        }
                        else { _logger.LogWarning("IA sugeriu Role {Role} p/ {Id}, mas nenhum analista encontrado.", respostaIaObj.RoleSugerida, chamadoId); }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro gerar/salvar resp IA {Id}", chamadoId);
                    chamado.Status = "Aguardando Analista";
                }
                chamado.Mensagens.Add(new MensagemChat { ChamadoId = chamadoId, Conteudo = respostaIaObj.TextoResposta, DataEnvio = DateTime.UtcNow.AddSeconds(1), RemetenteNome = "IA NextLayer" });
                await _context.SaveChangesAsync();
            }

            await _context.Entry(chamado).Collection(c => c.Mensagens).LoadAsync();
            if (chamado.AnalistaId.HasValue) await _context.Entry(chamado).Reference(c => c.Analista).LoadAsync();

            return MapearParaListaMensagemViewModel(chamado);
        }

        // --- Método EncontrarProximoAnalistaPorRoleAsync (Corrigido com 'LIKE') ---
        private async Task<Employee?> EncontrarProximoAnalistaPorRoleAsync(string roleSugerida)
        {
            _logger.LogInformation("Procurando próximo analista para Role: {Role}", roleSugerida);
            var roleLower = roleSugerida.ToLower();
            var analistasDaRole = await _context.Employees
                .Where(e => e.Role != null && e.Role.ToLower().Contains(roleLower)) // <-- Lógica LIKE
                .OrderBy(e => e.Name)
                .ToListAsync();
            if (!analistasDaRole.Any()) { _logger.LogWarning("Nenhum analista encontrado para Role (LIKE): {Role}", roleSugerida); return null; }
            if (analistasDaRole.Count == 1) { return analistasDaRole.First(); }
            var ultimoChamadoAtribuido = await _context.Chamados
                .Where(c => c.AnalistaId.HasValue && analistasDaRole.Select(a => a.Id).Contains(c.AnalistaId.Value))
                .OrderByDescending(c => c.DataAbertura).FirstOrDefaultAsync();
            if (ultimoChamadoAtribuido == null || !ultimoChamadoAtribuido.AnalistaId.HasValue) { return analistasDaRole.First(); }
            int ultimoIndice = analistasDaRole.FindIndex(a => a.Id == ultimoChamadoAtribuido.AnalistaId.Value);
            if (ultimoIndice == -1) { return analistasDaRole.First(); }
            int proximoIndice = (ultimoIndice + 1) % analistasDaRole.Count;
            return analistasDaRole[proximoIndice];
        }

        // --- Método GetChamadosEmAberto (Corrigido) ---
        public async Task<IEnumerable<ChamadoGridViewModel>> GetChamadosEmAberto()
        {
            return await _context.Chamados.Include(c => c.Cliente).Include(c => c.Analista)
                .Where(c => c.Status != "Fechado").OrderByDescending(c => c.Prioridade == "Alta" ? 3 : (c.Prioridade == "Média" ? 2 : 1)).ThenBy(c => c.DataAbertura)
                .Select(c => new ChamadoGridViewModel
                {
                    Id = c.Id,
                    NumeroChamado = c.NumeroChamado ?? "N/A",
                    Titulo = c.Titulo ?? "S/ Título",
                    NomeCliente = c.Cliente.Name ?? "Desc.",
                    DataAbertura = c.DataAbertura,
                    Status = c.Status ?? "N/A",
                    NomeAnalista = c.Analista == null ? "--" : c.Analista.Name
                }).ToListAsync();
        }

        // --- Método GetChamadosPorAnalistaAsync (Corrigido) ---
        public async Task<IEnumerable<ChamadoGridViewModel>> GetChamadosPorAnalistaAsync(int analistaId)
        {
            return await _context.Chamados.Include(c => c.Cliente)
                .Where(c => c.AnalistaId == analistaId && c.Status != "Fechado" && c.Status != "Concluído" && c.Status != "Cancelado")
                .OrderByDescending(c => c.Prioridade == "Alta" ? 3 : (c.Prioridade == "Média" ? 2 : 1)).ThenBy(c => c.DataAbertura)
                .Select(c => new ChamadoGridViewModel
                {
                    Id = c.Id,
                    NumeroChamado = c.NumeroChamado ?? "N/A",
                    Titulo = c.Titulo ?? "S/ Título",
                    NomeCliente = c.Cliente.Name ?? "Desc.",
                    DataAbertura = c.DataAbertura,
                    Status = c.Status ?? "N/A",
                    NomeAnalista = null
                }).ToListAsync();
        }

        // --- Método GetChamadosPorCliente (Corrigido) ---
        public async Task<IEnumerable<ChamadoGridViewModel>> GetChamadosPorCliente(int clienteId)
        {
            return await _context.Chamados.Where(c => c.ClienteId == clienteId).OrderByDescending(c => c.DataAbertura)
                .Select(c => new ChamadoGridViewModel
                {
                    Id = c.Id,
                    NumeroChamado = c.NumeroChamado ?? "N/A",
                    Titulo = c.Titulo ?? "S/ Título",
                    NomeCliente = null,
                    DataAbertura = c.DataAbertura,
                    Status = c.Status ?? "N/A"
                }).ToListAsync();
        }

        // --- Método GetDetalheChamado (Corrigido) ---
        public async Task<DetalheChamadoViewModel> GetDetalheChamado(int chamadoId)
        {
            var chamado = await _context.Chamados.Include(c => c.Cliente).Include(c => c.Analista).Include(c => c.Mensagens.OrderBy(m => m.DataEnvio)).Include(c => c.Anexos).FirstOrDefaultAsync(c => c.Id == chamadoId);
            if (chamado == null) { return null!; }
            var nomeCliente = chamado.Cliente?.Name ?? "Cliente Desc.";
            return MapearParaDetalheViewModel(chamado, nomeCliente);
        }

        // --- Método AtualizarChamado (Corrigido) ---
        public async Task<Chamado> AtualizarChamado(int chamadoId, AtualizarChamadoViewModel model)
        {
            var chamado = await _context.Chamados.FindAsync(chamadoId);
            if (chamado == null) { throw new KeyNotFoundException("Chamado não encontrado."); }
            var statusAnterior = chamado.Status;
            chamado.Status = model.Status; chamado.Prioridade = model.Prioridade; chamado.RoleDesignada = model.RoleDesignada; chamado.AnalistaId = model.AnalistaId;
            if (chamado.Status == "Concluído" && statusAnterior != "Concluído" && !chamado.DataConclusao.HasValue)
            {
                chamado.DataConclusao = DateTime.UtcNow;
            }
            else if (statusAnterior == "Concluído" && chamado.Status != "Concluído" && chamado.Status != "Encerrado")
            {
                chamado.DataConclusao = null;
            }
            if (model.AnalistaId.HasValue && !chamado.AnalistaInteragiu) { chamado.AnalistaInteragiu = true; }
            _context.Chamados.Update(chamado); await _context.SaveChangesAsync();
            return chamado;
        }

        // --- Método Auxiliar MapearParaDetalheViewModel (Corrigido) ---
        private DetalheChamadoViewModel MapearParaDetalheViewModel(Chamado chamado, string nomeCliente)
        {
            string nomeAnalista = chamado.Analista?.Name ?? "Analista";
            return new DetalheChamadoViewModel
            {
                Id = chamado.Id,
                NumeroChamado = chamado.NumeroChamado ?? "N/A",
                Titulo = chamado.Titulo ?? "S/ Título",
                Descricao = chamado.Descricao ?? "S/ Descrição",
                DataAbertura = chamado.DataAbertura,
                Status = chamado.Status ?? "N/A",
                NomeCliente = nomeCliente,
                Prioridade = chamado.Prioridade ?? "Média",
                RoleDesignada = chamado.RoleDesignada,
                AnalistaId = chamado.AnalistaId,
                DataConclusao = chamado.DataConclusao,
                Anexos = chamado.Anexos?.Select(a => new AnexoViewModel { Id = a.Id, NomeArquivo = a.NomeArquivo ?? "arquivo", UrlArquivo = a.UrlArquivo ?? "#" }).ToList() ?? new List<AnexoViewModel>(),
                Mensagens = MapearParaListaMensagemViewModel(chamado)
            };
        }

        // --- Método Auxiliar MapearParaListaMensagemViewModel (Corrigido) ---
        private List<MensagemViewModel> MapearParaListaMensagemViewModel(Chamado chamado)
        {
            string nomeCliente = chamado.Cliente?.Name ?? "Cliente";
            string nomeAnalista = chamado.Analista?.Name ?? "Analista";
            return chamado.Mensagens.OrderBy(m => m.DataEnvio).Select(m => new MensagemViewModel
            {
                Id = m.Id,
                Conteudo = m.Conteudo ?? "",
                DataEnvio = m.DataEnvio,
                RemetenteNome = m.RemetenteNome ?? (m.ClienteRemetenteId.HasValue ? nomeCliente : (m.FuncionarioRemetenteId.HasValue ? nomeAnalista : "IA NextLayer")),
                TipoRemetente = m.ClienteRemetenteId.HasValue ? "Client" : (m.FuncionarioRemetenteId.HasValue ? "Employee" : "IA")
            }).ToList();
        }
    }
}