// --- ARQUIVO: Services/GeminiIaService.cs (USANDO LÓGICA Groq) ---

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NextLayer.Models;
using NextLayer.ViewModels; // Necessário para IaResposta
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Mscc.GenerativeAI; // <--- USA O PACOTE CORRETO

namespace NextLayer.Services
{
    public class GeminiIaService : IIaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly ILogger<GeminiIaService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _modelName = "llama-3.1-8b-instant"; // Modelo Groq ativo

        public GeminiIaService(IConfiguration configuration,
                               IHttpClientFactory clientFactory,
                               ILogger<GeminiIaService> logger)
        {
            _logger = logger;
            _apiKey = configuration["Groq:ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogError("ERRO CRÍTICO: Groq:ApiKey não encontrada no appsettings.json.");
                throw new ArgumentNullException("Groq:ApiKey não encontrada.");
            }

            _httpClient = clientFactory.CreateClient("GroqApiClient");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _apiUrl = "https://api.groq.com/openai/v1/chat/completions";

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
            _logger.LogInformation("GeminiIaService (API REST Groq) inicializado. Modelo: {ModelName}", _modelName);
        }

        public async Task<IaResposta> GerarRespostaAsync(Chamado chamado, string novaMensagemCliente)
        {
            _logger.LogInformation("Gerando resposta da Groq para ChamadoId {ChamadoId}", chamado.Id);
            var systemInstruction = @"Você é um assistente de suporte técnico da empresa NextLayer.
Seu objetivo é resolver o problema do cliente em português do Brasil.
Categorias de Suporte Válidas: [Infraestrutura, Software, Hardware, Rede, Senhas, Outros]
REGRAS DE RESPOSTA:
1. Responda de forma educada e prestativa.
2. Se o usuário pedir explicitamente por um 'humano', 'atendente' ou 'analista':
    - Sua resposta DEVE ser: 'Entendido. Estou encaminhando seu chamado para um analista.'
    - Você DEVE determinar a categoria do problema (ex: Infraestrutura, Software) com base em todo o histórico.
    - Você DEVE retornar a resposta no formato: [RESPOSTA] TEXTO_DA_RESPOSTA [CATEGORIA] NOME_DA_CATEGORIA
3. Se o usuário NÃO pedir um analista:
    - Tente resolver o problema.
    - Você NÃO precisa retornar a [CATEGORIA].
    - Exemplo: [RESPOSTA] Tente reiniciar seu computador.
";

            var messages = new List<GroqMessage>();
            messages.Add(new GroqMessage { Role = "system", Content = systemInstruction });
            messages.Add(new GroqMessage { Role = "system", Content = $"Contexto: Título: {chamado.Titulo}\nDescrição: {chamado.Descricao}" });

            foreach (var msg in chamado.Mensagens.OrderBy(m => m.DataEnvio))
            {
                string role = (msg.ClienteRemetenteId.HasValue || msg.FuncionarioRemetenteId.HasValue) ? "user" : "assistant";
                string prefix = msg.FuncionarioRemetenteId.HasValue ? "(Analista): " : "";
                messages.Add(new GroqMessage { Role = role, Content = prefix + msg.Conteudo });
            }
            messages.Add(new GroqMessage { Role = "user", Content = novaMensagemCliente });

            var requestBody = new GroqRequest { Messages = messages, Model = _modelName, Temperature = 0.5f };

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiUrl, requestBody, _jsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    var groqResponse = await response.Content.ReadFromJsonAsync<GroqResponse>(_jsonOptions);
                    string? responseText = groqResponse?.Choices?.FirstOrDefault()?.Message?.Content;
                    if (!string.IsNullOrEmpty(responseText))
                    {
                        _logger.LogInformation("SUCESSO: Resposta Groq extraída.");
                        return ParseIaResposta(responseText);
                    }
                    else { return new IaResposta { TextoResposta = "(IA respondeu, mas sem texto.)" }; }
                }
                else { string err = await response.Content.ReadAsStringAsync(); _logger.LogError("Erro API Groq ({Code})... Conteúdo: {Err}", response.StatusCode, err); return new IaResposta { TextoResposta = $"Erro IA ({response.ReasonPhrase}). Ver logs." }; }
            }
            catch (Exception ex) { _logger.LogError(ex, "Erro inesperado API Groq."); return new IaResposta { TextoResposta = $"Erro inesperado IA: {ex.Message}" }; }
        }

        private IaResposta ParseIaResposta(string rawText)
        {
            var resposta = new IaResposta();
            var matchCategoria = Regex.Match(rawText, @"\[CATEGORIA\]\s*([A-Za-zçã]+)", RegexOptions.IgnoreCase);
            if (matchCategoria.Success)
            {
                resposta.RoleSugerida = matchCategoria.Groups[1].Value.Trim();
                resposta.DeveEncaminhar = true;
                _logger.LogInformation("IA sugeriu a Role: {Role}", resposta.RoleSugerida);
            }
            var matchResposta = Regex.Match(rawText, @"\[RESPOSTA\]\s*(.+)", RegexOptions.Singleline);
            if (matchResposta.Success)
            {
                string texto = matchResposta.Groups[1].Value.Trim();
                resposta.TextoResposta = Regex.Replace(texto, @"\[CATEGORIA\].*", "").Trim();
            }
            else
            {
                resposta.TextoResposta = Regex.Replace(rawText, @"\[CATEGORIA\].*", "").Trim();
            }
            if (resposta.TextoResposta.Contains("encaminhando seu chamado") && !resposta.DeveEncaminhar)
            {
                resposta.DeveEncaminhar = true;
            }
            return resposta;
        }

        // --- Classes Auxiliares JSON (Groq/OpenAI) ---
        private class GroqRequest { [JsonPropertyName("messages")] public List<GroqMessage> Messages { get; set; } = new(); [JsonPropertyName("model")] public string Model { get; set; } = string.Empty; [JsonPropertyName("temperature")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public float Temperature { get; set; } = 0.7f; }
        private class GroqMessage { [JsonPropertyName("role")] public string Role { get; set; } = string.Empty; [JsonPropertyName("content")] public string Content { get; set; } = string.Empty; }
        private class GroqResponse { [JsonPropertyName("choices")] public List<GroqChoice>? Choices { get; set; } }
        private class GroqChoice { [JsonPropertyName("message")] public GroqMessage? Message { get; set; } }
        private class GroqUsage { }
    }
}