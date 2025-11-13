using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NextLayer.Services;
using NextLayer.ViewModels; // Pode precisar para o request de sugestão
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NextLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FaqController : ControllerBase
    {
        private readonly IFaqService _faqService;
        private readonly ILogger<FaqController> _logger;

        public FaqController(IFaqService faqService, ILogger<FaqController> logger)
        {
            _faqService = faqService;
            _logger = logger;
        }

        /// <summary>
        /// Retorna todos os itens de FAQ cadastrados.
        /// </summary>
        [HttpGet] // Rota: GET /api/faq
        public async Task<IActionResult> GetAllFaqs()
        {
            try
            {
                var faqs = await _faqService.GetAllFaqsAsync();
                // Retorna apenas os dados necessários (evita retornar objetos complexos se houver)
                var result = faqs.Select(f => new { f.Id, f.Pergunta, f.Resposta }).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os FAQs.");
                return StatusCode(500, "Erro interno ao buscar FAQs.");
            }
        }

        /// <summary>
        /// ViewModel para receber os dados de sugestão de FAQ.
        /// </summary>
        public class SugestaoFaqRequest
        {
            public string? Titulo { get; set; }
            public string? Descricao { get; set; }
        }

        /// <summary>
        /// Sugere itens de FAQ com base no título e descrição fornecidos.
        /// </summary>
        [HttpPost("sugerir")] // Rota: POST /api/faq/sugerir
        public async Task<IActionResult> GetSugestoes([FromBody] SugestaoFaqRequest request)
        {
            // Validação básica
            if (string.IsNullOrWhiteSpace(request.Titulo) && string.IsNullOrWhiteSpace(request.Descricao))
            {
                return BadRequest("Título ou Descrição devem ser fornecidos para sugestão.");
            }

            try
            {
                // Garante que não passamos null para o serviço
                var titulo = request.Titulo ?? string.Empty;
                var descricao = request.Descricao ?? string.Empty;

                var sugestoes = await _faqService.GetFaqSugestoesAsync(titulo, descricao);
                // Retorna apenas os dados necessários
                var result = sugestoes.Select(f => new { f.Id, f.Pergunta, f.Resposta }).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar sugestões de FAQ para Titulo='{Titulo}'", request.Titulo);
                return StatusCode(500, "Erro interno ao buscar sugestões.");
            }
        }
    }
}