// --- ARQUIVO: Services/ChamadoService.cs (Completo com Corpos dos Métodos) ---

using Microsoft.EntityFrameworkCore;
using NextLayer.Data;
using NextLayer.Models;
using NextLayer.ViewModels;
using shortid; // Namespace correto é minúsculo
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
            _iaService = iaService; // Interface original
            _logger = logger;
        }

        // --- Método CriarNovoChamado ---
        public async Task<DetalheChamadoViewModel> CriarNovoChamado(CriarChamadoViewModel model, int clienteId)
        {
            var cliente = await _context.Clients.FindAsync(clienteId);
            if (cliente == null)
            {
                _logger.LogError("ClienteId {ClienteId} inexistente ao criar chamado.", clienteId);
                throw new KeyNotFoundException("Cliente não encontrado.");
            }
            _logger.LogInformation("Criando chamado para ClienteId {ClienteId}", clienteId);

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

            if (model.Imagens != null && model.Imagens.Count > 0)
            {
                _logger.LogInformation("Processando {Count} anexos para {NumeroChamado}", model.Imagens.Count, novoChamado.NumeroChamado);
                foreach (var imagem in model.Imagens)
                {
                    try
                    {
                        var diretorio = $"uploads/chamados/{novoChamado.NumeroChamado}";
                        var urlArquivo = await _fileStorage.SalvarArquivo(imagem, diretorio);
                        novoChamado.Anexos.Add(new Anexo { NomeArquivo = imagem.FileName, UrlArquivo = urlArquivo, TipoConteudo = imagem.ContentType, DataUpload = DateTime.UtcNow });
                    }
                    catch (Exception ex) { _logger.LogError(ex, "Erro ao salvar anexo {FileName}", imagem.FileName); }
                }
            }

            novoChamado.Mensagens.Add(new MensagemChat { Conteudo = $"Problema inicial: {model.Descricao}", DataEnvio = DateTime.UtcNow, ClienteRemetenteId = clienteId, RemetenteNome = cliente.Name });

            _logger.LogInformation("Gerando 1ª resposta IA para {NumeroChamado}", novoChamado.NumeroChamado);
            string respostaIa = "(IA não pôde ser contatada)"; // Default message
            try
            {
                respostaIa = await _iaService.GerarRespostaAsync(novoChamado, model.Descricao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro gerar 1ª resposta IA para {NumeroChamado}", novoChamado.NumeroChamado);
                novoChamado.Status = "Aguardando Analista"; // Encaminha se IA falhar
            }
            // Adiciona a resposta (ou a mensagem de erro)
            novoChamado.Mensagens.Add(new MensagemChat { Conteudo = respostaIa, DataEnvio = DateTime.UtcNow.AddSeconds(1), RemetenteNome = "IA NextLayer" });

            _context.Chamados.Add(novoChamado);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Chamado {NumeroChamado} (ID {ChamadoId}) criado.", novoChamado.NumeroChamado, novoChamado.Id);
            // Chama o helper para retornar o ViewModel
            return MapearParaDetalheViewModel(novoChamado, cliente.Name);
        }

        // --- Método AdicionarMensagem ---
        public async Task<List<MensagemViewModel>> AdicionarMensagem(int chamadoId, string conteudo, int remetenteId, string tipoRemetente)
        {
            _logger.LogInformation("Adicionando msg ChamadoId {Id} por {Tipo} ID {RemId}", chamadoId, tipoRemetente, remetenteId);
            var chamado = await _context.Chamados.Include(c => c.Mensagens).Include(c => c.Cliente).Include(c => c.Analista).FirstOrDefaultAsync(c => c.Id == chamadoId);
            if (chamado == null) { _logger.LogWarning("ChamadoId {Id} não encontrado.", chamadoId); throw new KeyNotFoundException("Chamado não encontrado."); }

            string nomeCliente = chamado.Cliente?.Name ?? "Cliente";
            string nomeRemetente = nomeCliente;
            bool deveChamarIa = false;
            var novaMensagem = new MensagemChat { ChamadoId = chamadoId, Conteudo = conteudo, DataEnvio = DateTime.UtcNow };

            if (tipoRemetente == "Client")
            {
                novaMensagem.ClienteRemetenteId = remetenteId; novaMensagem.RemetenteNome = nomeCliente;
                if (!chamado.AnalistaInteragiu) { _logger.LogInformation("IA responderá cliente ChamadoId {Id}", chamadoId); deveChamarIa = true; }
                else { _logger.LogInformation("IA NÃO responderá ChamadoId {Id} (analista interagiu)", chamadoId); }
            }
            else
            { // Employee
                var analista = await _context.Employees.FindAsync(remetenteId); nomeRemetente = analista?.Name ?? "Analista";
                novaMensagem.FuncionarioRemetenteId = remetenteId; novaMensagem.RemetenteNome = nomeRemetente;
                if (!chamado.AnalistaInteragiu) { _logger.LogInformation("1ª interação Analista {IdA} ChamadoId {IdC}. Desativando IA.", remetenteId, chamadoId); chamado.AnalistaInteragiu = true; }
                if (chamado.Status.Contains("IA") || chamado.Status.Contains("Aguardando")) { chamado.Status = "Em Andamento (Analista)"; }
                if (!chamado.AnalistaId.HasValue || chamado.AnalistaId != remetenteId) { chamado.AnalistaId = remetenteId; _logger.LogInformation("ChamadoId {IdC} atribuído Analista {IdA}", chamadoId, remetenteId); }
            }
            chamado.Mensagens.Add(novaMensagem);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Msg salva ChamadoId {Id}", chamadoId);

            if (deveChamarIa)
            {
                _logger.LogInformation("Gerando resposta IA para ChamadoId {Id}", chamadoId);
                string respostaIa = "(Erro ao contatar IA)"; // Default
                try
                {
                    respostaIa = await _iaService.GerarRespostaAsync(chamado, conteudo);
                    if (respostaIa.Contains("encaminhando seu chamado")) { _logger.LogInformation("IA encaminhou ChamadoId {Id}", chamadoId); chamado.Status = "Aguardando Analista"; }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro gerar/salvar resposta IA ChamadoId {Id}", chamadoId);
                    chamado.Status = "Aguardando Analista"; // Encaminha se IA falhar
                }
                // Adiciona a resposta (ou a mensagem de erro)
                chamado.Mensagens.Add(new MensagemChat { ChamadoId = chamadoId, Conteudo = respostaIa, DataEnvio = DateTime.UtcNow.AddSeconds(1), RemetenteNome = "IA NextLayer" });
                await _context.SaveChangesAsync();
                _logger.LogInformation("Resposta IA (ou erro) salva ChamadoId {Id}", chamadoId);
            }

            _logger.LogDebug("Recarregando msgs ChamadoId {Id}", chamadoId);
            await _context.Entry(chamado).Collection(c => c.Mensagens).LoadAsync();
            await _context.Entry(chamado).Reference(c => c.Analista).LoadAsync(); // Garante que o analista está carregado
            string nomeAnalista = chamado.Analista?.Name ?? "Analista";

            // Retorna a lista mapeada
            return chamado.Mensagens.OrderBy(m => m.DataEnvio).Select(m => new MensagemViewModel
            {
                Id = m.Id,
                Conteudo = m.Conteudo ?? "",
                DataEnvio = m.DataEnvio,
                RemetenteNome = m.RemetenteNome ?? (m.ClienteRemetenteId.HasValue ? nomeCliente : (m.FuncionarioRemetenteId.HasValue ? nomeAnalista : "IA NextLayer")),
                TipoRemetente = m.ClienteRemetenteId.HasValue ? "Client" : (m.FuncionarioRemetenteId.HasValue ? "Employee" : "IA")
            }).ToList();
        }

        // --- Método GetChamadosEmAberto ---
        public async Task<IEnumerable<ChamadoGridViewModel>> GetChamadosEmAberto()
        {
            _logger.LogInformation("Buscando chamados em aberto para o grid do analista.");
            return await _context.Chamados
                .Include(c => c.Cliente)
                .Where(c => c.Status != "Fechado")
                .OrderByDescending(c => c.Prioridade == "Alta" ? 3 : (c.Prioridade == "Média" ? 2 : 1))
                .ThenBy(c => c.DataAbertura)
                .Select(c => new ChamadoGridViewModel
                {
                    Id = c.Id,
                    NumeroChamado = c.NumeroChamado ?? "N/A",
                    Titulo = c.Titulo ?? "Sem Título",
                    NomeCliente = c.Cliente.Name ?? "Cliente Desconhecido",
                    DataAbertura = c.DataAbertura,
                    Status = c.Status ?? "N/A"
                })
                .ToListAsync();
        }

        // --- Método GetChamadosPorCliente ---
        public async Task<IEnumerable<ChamadoGridViewModel>> GetChamadosPorCliente(int clienteId)
        {
            _logger.LogInformation("Buscando chamados para ClienteId {ClienteId}", clienteId);
            return await _context.Chamados
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.DataAbertura)
                .Select(c => new ChamadoGridViewModel
                {
                    Id = c.Id,
                    NumeroChamado = c.NumeroChamado ?? "N/A",
                    Titulo = c.Titulo ?? "Sem Título",
                    NomeCliente = null, // Cliente já sabe que é dele
                    DataAbertura = c.DataAbertura,
                    Status = c.Status ?? "N/A"
                })
                .ToListAsync();
        }

        // --- Método GetDetalheChamado ---
        public async Task<DetalheChamadoViewModel> GetDetalheChamado(int chamadoId)
        {
            _logger.LogInformation("Buscando detalhes para ChamadoId {ChamadoId}", chamadoId);
            var chamado = await _context.Chamados
                .Include(c => c.Cliente)
                .Include(c => c.Analista)
                .Include(c => c.Mensagens.OrderBy(m => m.DataEnvio))
                .Include(c => c.Anexos)
                .FirstOrDefaultAsync(c => c.Id == chamadoId);

            if (chamado == null)
            {
                _logger.LogWarning("ChamadoId {ChamadoId} não encontrado.", chamadoId);
                // Retornar null faz o Controller retornar 404 Not Found
                return null!; // Usamos ! para suprimir o aviso, pois o controller trata null
            }

            var nomeCliente = chamado.Cliente?.Name ?? "Cliente Desconhecido";
            return MapearParaDetalheViewModel(chamado, nomeCliente);
        }

        // --- Método AtualizarChamado ---
        public async Task<Chamado> AtualizarChamado(int chamadoId, AtualizarChamadoViewModel model)
        {
            _logger.LogInformation("Atualizando ChamadoId {Id} com Status={Status}, Prioridade={Prio}",
               chamadoId, model.Status, model.Prioridade);
            var chamado = await _context.Chamados.FindAsync(chamadoId);
            if (chamado == null)
            {
                _logger.LogWarning("Tentativa de atualizar ChamadoId {Id} inexistente.", chamadoId);
                throw new KeyNotFoundException("Chamado não encontrado.");
            }

            chamado.Status = model.Status;
            chamado.Prioridade = model.Prioridade;
            chamado.RoleDesignada = model.RoleDesignada;
            chamado.AnalistaId = model.AnalistaId;

            if (model.AnalistaId.HasValue && !chamado.AnalistaInteragiu)
            {
                _logger.LogInformation("Marcando ChamadoId {Id} como interagido (atribuição).", chamadoId);
                chamado.AnalistaInteragiu = true;
            }

            _context.Chamados.Update(chamado);
            await _context.SaveChangesAsync();
            _logger.LogInformation("ChamadoId {Id} atualizado.", chamadoId);
            return chamado; // Retorna a entidade atualizada
        }

        // --- Método Auxiliar MapearParaDetalheViewModel ---
        private DetalheChamadoViewModel MapearParaDetalheViewModel(Chamado chamado, string nomeCliente)
        {
            string nomeAnalista = chamado.Analista?.Name ?? "Analista";
            return new DetalheChamadoViewModel
            {
                Id = chamado.Id,
                NumeroChamado = chamado.NumeroChamado ?? "N/A",
                Titulo = chamado.Titulo ?? "Sem Título",
                Descricao = chamado.Descricao ?? "Sem Descrição",
                DataAbertura = chamado.DataAbertura,
                Status = chamado.Status ?? "N/A",
                NomeCliente = nomeCliente,
                Anexos = chamado.Anexos?.Select(a => new AnexoViewModel { Id = a.Id, NomeArquivo = a.NomeArquivo ?? "arquivo", UrlArquivo = a.UrlArquivo ?? "#" }).ToList() ?? new List<AnexoViewModel>(),
                Mensagens = chamado.Mensagens?.OrderBy(m => m.DataEnvio).Select(m => new MensagemViewModel
                {
                    Id = m.Id,
                    Conteudo = m.Conteudo ?? "",
                    DataEnvio = m.DataEnvio,
                    RemetenteNome = m.RemetenteNome ?? (m.ClienteRemetenteId.HasValue ? nomeCliente : (m.FuncionarioRemetenteId.HasValue ? nomeAnalista : "IA NextLayer")),
                    TipoRemetente = m.ClienteRemetenteId.HasValue ? "Client" : (m.FuncionarioRemetenteId.HasValue ? "Employee" : "IA")
                }).ToList() ?? new List<MensagemViewModel>()
            };
        }
    }
}