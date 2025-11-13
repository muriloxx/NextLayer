using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NextLayer.Services;
using NextLayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Para futura autenticação
using System.Security.Claims; // Para futura autenticação
using Microsoft.EntityFrameworkCore;

namespace NextLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protege TODOS os endpoints por padrão
    public class ChamadoController : ControllerBase
    {
        private readonly IChamadoService _chamadoService;
        private readonly ILogger<ChamadoController> _logger;

        public ChamadoController(IChamadoService chamadoService, ILogger<ChamadoController> logger)
        {
            _chamadoService = chamadoService;
            _logger = logger;
        }

        // ---  MÉTODO PARA O DASHBOARD ADMIN ---
        [HttpGet("todos-admin")]
        [Authorize(Policy = "AdminOnly")] // Apenas Admins podem ver tudo
        public async Task<IActionResult> GetAllChamadosParaAdmin([FromQuery] string? status, [FromQuery] string? prioridade)
        {
            try
            {
                // Agora chama o Service, que tem a lógica correta
                var chamados = await _chamadoService.GetAllChamadosAdminAsync(status, prioridade);
                return Ok(chamados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao buscar chamados.", error = ex.Message });
            }
        }
        // --- FIM DO  MÉTODO ---

        // ---  MÉTODO HELPER (Usado internamente) ---
        /// <summary>
        /// Obtém o ID e o Tipo (Role) do usuário logado a partir do token JWT.
        /// </summary>
        private (int Id, string Tipo) GetUsuarioLogado()
        {
            // O HttpContext.User é preenchido pelo middleware [Authorize]
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userRoleClaim = User.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null || userRoleClaim == null)
            {
                // Isso não deve acontecer se [Authorize] estiver ativo,
                // mas é uma boa verificação de segurança.
                throw new InvalidOperationException("Token inválido ou não contém ID/Role.");
            }

            return (int.Parse(userIdClaim.Value), userRoleClaim.Value);
        }

        // --- ENDPOINT DO CLIENTE (CRIAR) ---
        [HttpPost("criar")]
        [Authorize(Roles = "Client")] // Apenas Clientes podem criar
        public async Task<IActionResult> CriarChamado([FromForm] CriarChamadoViewModel model)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            try
            {
                var (clienteIdLogado, _) = GetUsuarioLogado(); // Pega o ID real do token
                _logger.LogInformation("Req criar chamado ClienteId {Id}: {Titulo}", clienteIdLogado, model.Titulo);

                var novoChamadoVM = await _chamadoService.CriarNovoChamado(model, clienteIdLogado);
                return CreatedAtAction(nameof(GetDetalheChamado), new { id = novoChamadoVM.Id }, novoChamadoVM);
            }
            catch (KeyNotFoundException knfEx) { return NotFound(new { message = knfEx.Message }); }
            catch (Exception ex) { _logger.LogError(ex, "Erro criar chamado."); return StatusCode(500, ex.Message); }
        }

        // --- ENDPOINT DO CLIENTE (MEUS CHAMADOS) ---
        [HttpGet("meuschamados")]
        [Authorize(Roles = "Client")] // Apenas Clientes
        public async Task<IActionResult> GetMeusChamados()
        {
            try
            {
                var (clienteIdLogado, _) = GetUsuarioLogado(); // Pega o ID real do token
                _logger.LogInformation("Req meus chamados ClienteId {Id}", clienteIdLogado);
                var chamados = await _chamadoService.GetChamadosPorCliente(clienteIdLogado);
                return Ok(chamados);
            }
            catch (Exception ex) { _logger.LogError(ex, "Erro buscar meus chamados"); return StatusCode(500, ex.Message); }
        }

        // --- ENDPOINT DO ANALISTA (MEUS CHAMADOS ATRIBUÍDOS) ---
        [HttpGet("meus-chamados-analista")]
        [Authorize(Roles = "Employee")] // Apenas Funcionários
        public async Task<IActionResult> GetMeusChamadosAnalista()
        {
            try
            {
                var (analistaIdLogado, _) = GetUsuarioLogado(); // Pega o ID real do token
                _logger.LogInformation("Req meus chamados AnalistaId {Id}", analistaIdLogado);
                var chamados = await _chamadoService.GetChamadosPorAnalistaAsync(analistaIdLogado);
                return Ok(chamados);
            }
            catch (Exception ex) { _logger.LogError(ex, "Erro buscar chamados atribuídos"); return StatusCode(500, ex.Message); }
        }

        // --- ENDPOINT DO ANALISTA (GRID TODOS) ---
        [HttpGet("abertos")]
        [Authorize(Roles = "Employee")] // Apenas Funcionários
        public async Task<IActionResult> GetChamadosAbertos()
        {
            _logger.LogInformation("Req grid analista.");
            try
            {
                var chamados = await _chamadoService.GetChamadosEmAberto();
                return Ok(chamados);
            }
            catch (Exception ex) { _logger.LogError(ex, "Erro buscar chamados abertos."); return StatusCode(500, "Erro interno."); }
        }

        // --- ENDPOINT GERAL (DETALHE/CHAT) ---
        [HttpGet("{id}")]
        // [Authorize] // Já está protegido no nível da classe
        public async Task<IActionResult> GetDetalheChamado(int id)
        {
            _logger.LogInformation("Req detalhes ChamadoId {Id}", id);
            try
            {
                var chamadoVM = await _chamadoService.GetDetalheChamado(id);
                if (chamadoVM == null) { _logger.LogWarning("ChamadoId {Id} não encontrado.", id); return NotFound(); }
                // TODO: Adicionar verificação se o Cliente logado é o dono do chamado
                return Ok(chamadoVM);
            }
            catch (Exception ex) { _logger.LogError(ex, "Erro buscar detalhes {Id}", id); return StatusCode(500, "Erro interno."); }
        }

        // --- ENDPOINT GERAL (ENVIAR MENSAGEM) ---
        [HttpPost("{id}/mensagem")]
        // [Authorize] // Já está protegido
        public async Task<IActionResult> AdicionarMensagem(int id, [FromBody] AdicionarMensagemViewModel model)
        {
            _logger.LogInformation("Req nova msg ChamadoId {Id}", id);
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            try
            {
                var (remetenteId, tipoRemetente) = GetUsuarioLogado(); // Pega ID e Role do token
                _logger.LogDebug("Add msg como {Tipo} ID {RId}", tipoRemetente, remetenteId);

                if (model.TipoRemetente != tipoRemetente)
                {
                    _logger.LogWarning("RISCO: TipoRemetente do front ({Front}) não bate com Token ({Token})", model.TipoRemetente, tipoRemetente);
                    return Forbid("Tipo de remetente inválido.");
                }

                var novasMensagens = await _chamadoService.AdicionarMensagem(id, model.Conteudo, remetenteId, tipoRemetente);
                return Ok(novasMensagens);
            }
            catch (KeyNotFoundException knfEx) { return NotFound(new { message = knfEx.Message }); }
            catch (InvalidOperationException ioEx) { _logger.LogWarning("Operação inválida msg ChamadoId {Id}: {Msg}", id, ioEx.Message); return StatusCode(StatusCodes.Status403Forbidden, new { message = ioEx.Message }); }
            catch (Exception ex) { _logger.LogError(ex, "Erro add msg {Id}", id); return StatusCode(500, "Erro interno."); }
        }

        // --- ENDPOINT DO ANALISTA (ATUALIZAR STATUS/PRIORIDADE) ---
        [HttpPut("{id}/atualizar")]
        [Authorize(Roles = "Employee")] // Apenas Funcionários
        public async Task<IActionResult> AtualizarChamado(int id, [FromBody] AtualizarChamadoViewModel model)
        {
            _logger.LogInformation("Req atualizar ChamadoId {Id}", id);
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            try
            {
                var chamado = await _chamadoService.AtualizarChamado(id, model);
                _logger.LogInformation("ChamadoId {Id} atualizado.", id);
                return Ok(chamado);
            }
            catch (KeyNotFoundException knfEx) { return NotFound(new { message = knfEx.Message }); }
            catch (Exception ex) { _logger.LogError(ex, "Erro atualizar {Id}", id); return StatusCode(500, "Erro interno."); }
        }
    }
}