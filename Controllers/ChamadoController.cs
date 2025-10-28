// --- ARQUIVO: Controllers/ChamadoController.cs ---

using Microsoft.AspNetCore.Authorization; // (Para futura autenticação)
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NextLayer.Services; // Importa nossos serviços
using NextLayer.ViewModels; // Importa nossos ViewModels
using System;
using System.Security.Claims; // (Para futura autenticação)
using System.Threading.Tasks;

namespace NextLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Define a rota base para este controller: /api/Chamado
    // [Authorize] // (RECOMENDAÇÃO: Descomente esta linha quando tiver JWT. Isso protege todos os endpoints)
    public class ChamadoController : ControllerBase
    {
        // O controller depende do serviço de chamados.
        // O 'readonly' garante que ele só pode ser definido no construtor.
        private readonly IChamadoService _chamadoService;

        // Construtor usado pela Injeção de Dependência (Program.cs)
        // O ASP.NET "injeta" automaticamente o ChamadoService aqui.
        public ChamadoController(IChamadoService chamadoService)
        {
            _chamadoService = chamadoService;
        }

        // --- ENDPOINT DO CLIENTE (CRIAR) ---
        // Rota: POST /api/Chamado/criar
        // [FromForm] é usado porque este endpoint aceita upload de arquivos (Imagens)
        [HttpPost("criar")]
        // [Authorize(Roles = "Client")] // (No futuro, garanta que só Clientes podem criar)
        public async Task<IActionResult> CriarChamado([FromForm] CriarChamadoViewModel model)
        {
            // Validação automática com base nos [Required] do ViewModel
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // ---- NOTA DE AUTENTICAÇÃO (SIMULAÇÃO) ----
                // No futuro, você pegará o ID do cliente do Token JWT, assim:
                // var clienteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // int.TryParse(clienteId, out int id);

                // Por enquanto, "fingimos" ser o cliente ID=1.
                // AJUSTE ESTE ID se o seu cliente de teste tiver outro ID no banco!
                int clienteIdLogado = 1;
                // ---------------------------------

                // Chama o serviço, que agora retorna o ViewModel de detalhe (sem loops)
                var novoChamadoVM = await _chamadoService.CriarNovoChamado(model, clienteIdLogado);

                // Retorna 201 Created (Sucesso)
                // Inclui a rota para acessar o novo chamado (no cabeçalho 'Location')
                // E inclui o próprio chamado (já com a primeira resposta da IA) no corpo.
                return CreatedAtAction(nameof(GetDetalheChamado), new { id = novoChamadoVM.Id }, novoChamadoVM);
            }
            catch (Exception ex)
            {
                // Captura qualquer erro inesperado (ex: falha ao salvar no banco)
                return StatusCode(500, $"Erro interno ao criar chamado: {ex.Message}");
            }
        }

        // --- ENDPOINT DO CLIENTE (LISTAR "MEUS CHAMADOS") ---
        // Rota: GET /api/Chamado/meuschamados
        [HttpGet("meuschamados")]
        // [Authorize(Roles = "Client")]
        public async Task<IActionResult> GetMeusChamados()
        {
            // ---- NOTA DE AUTENTICAÇÃO (SIMULAÇÃO) ----
            // "Fingindo" o ID do cliente logado
            int clienteIdLogado = 1; // AJUSTE ESTE ID se o seu cliente de teste tiver outro ID
                                     // ---------------------------------

            var chamados = await _chamadoService.GetChamadosPorCliente(clienteIdLogado);
            return Ok(chamados); // Retorna 200 OK com a lista de chamados
        }

        // --- ENDPOINT DO ANALISTA (LISTAR GRID) ---
        // Rota: GET /api/Chamado/abertos
        [HttpGet("abertos")]
        // [Authorize(Roles = "Employee")] // (No futuro, garanta que só Funcionários podem ver)
        public async Task<IActionResult> GetChamadosAbertos()
        {
            // Este método agora funciona, pois removemos o ReferenceHandler.Preserve
            // e o IChamadoService já retorna um ViewModel (ChamadoGridViewModel)
            var chamados = await _chamadoService.GetChamadosEmAberto();
            return Ok(chamados); // Retorna 200 OK com a lista
        }

        // --- ENDPOINT GERAL (DETALHE/CHAT) ---
        // Rota: GET /api/Chamado/5 (onde 5 é o ID do chamado)
        [HttpGet("{id}")]
        // [Authorize] // (Cliente ou Analista podem ver)
        public async Task<IActionResult> GetDetalheChamado(int id)
        {
            // O serviço agora retorna o DetalheChamadoViewModel, que não tem loops
            var chamadoVM = await _chamadoService.GetDetalheChamado(id);

            if (chamadoVM == null)
                return NotFound(); // Retorna 404 se o chamado não existe

            // Retorna 200 OK com os detalhes (incluindo anexos e histórico do chat)
            return Ok(chamadoVM);
        }

        // --- ENDPOINT GERAL (ENVIAR MENSAGEM NO CHAT) ---
        // Rota: POST /api/Chamado/5/mensagem
        [HttpPost("{id}/mensagem")]
        // [Authorize] // (Cliente ou Analista podem enviar)
        public async Task<IActionResult> AdicionarMensagem(int id, [FromBody] AdicionarMensagemViewModel model)
        {
            // Valida se o ViewModel (Conteudo, TipoRemetente) foi enviado corretamente
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // ---- NOTA DE AUTENTICAÇÃO (SIMULAÇÃO) ----
                // "Fingindo" o ID do remetente
                // (Aqui teríamos uma lógica para pegar o ID do Client ou do Employee logado)
                int remetenteId = 1; // Ajuste se seu usuário de teste não for ID=1

                // CORREÇÃO: Usamos o TipoRemetente enviado pelo front-end
                // Isso permite ao serviço saber se a IA deve ou não responder.
                string tipoRemetente = model.TipoRemetente;
                // ---------------------------------

                // O serviço salva a mensagem e (se for cliente) gera a resposta da IA
                var novasMensagens = await _chamadoService.AdicionarMensagem(id, model.Conteudo, remetenteId, tipoRemetente);

                // Retorna 200 OK com a lista de mensagens atualizada
                return Ok(novasMensagens);
            }
            catch (KeyNotFoundException ex)
            {
                // Erro se o chamado 'id' não for encontrado
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Erro genérico
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        // --- ENDPOINT DO ANALISTA (ATUALIZAR STATUS/PRIORIDADE) ---
        // Rota: PUT /api/Chamado/5/atualizar
        [HttpPut("{id}/atualizar")]
        // [Authorize(Roles = "Employee")]
        public async Task<IActionResult> AtualizarChamado(int id, [FromBody] AtualizarChamadoViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // O serviço vai atualizar Status, Prioridade, RoleDesignada, etc.
                var chamado = await _chamadoService.AtualizarChamado(id, model);
                return Ok(chamado); // Retorna 200 OK com o chamado atualizado
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}