// --- ARQUIVO: Services/GeminiIaService.cs (Nome original, lógica Groq) ---

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NextLayer.Models;
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

namespace NextLayer.Services
{
    // Mantém o nome da classe original
    // Implementa a interface original IIaService
    public class GeminiIaService : IIaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        // Logger usa o nome da classe original
        private readonly ILogger<GeminiIaService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _modelName = "llama-3.1-8b-instant"; // Modelo Groq ativo

        // Construtor usa Logger<GeminiIaService>
        public GeminiIaService(IConfiguration configuration,
                               IHttpClientFactory clientFactory,
                               ILogger<GeminiIaService> logger)
        {
            _logger = logger;
            _apiKey = configuration["Groq:ApiKey"]; // Lê a chave da Groq
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogError("ERRO CRÍTICO: Groq:ApiKey não encontrada no appsettings.json.");
                throw new ArgumentNullException("Groq:ApiKey não encontrada.");
            }

            _httpClient = clientFactory.CreateClient("GroqApiClient");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey); // Configura autenticação
            _apiUrl = "https://api.groq.com/openai/v1/chat/completions"; // URL Groq

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, // Para ler a resposta
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Não envia nulos
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower // Groq usa snake_case
            };

            // Mensagem de log ajustada para indicar que usa Groq internamente
            _logger.LogInformation("GeminiIaService (using Groq API REST) inicializado. Modelo: {ModelName}", _modelName);
        }

        // Método GerarRespostaAsync (implementa IIaService)
        public async Task<string> GerarRespostaAsync(Chamado chamado, string novaMensagemCliente)
        {
            _logger.LogInformation("Gerando resposta da Groq para ChamadoId {ChamadoId}", chamado.Id);
            var systemInstruction = @"Você é um assistente de suporte técnico da empresa NextLayer. Seu nome é 'IA NextLayer'.
Seu objetivo é resolver o problema do cliente. Responda em português do Brasil.
Seja educado, prestativo e técnico.
REGRA IMPORTANTE: Se o usuário pedir explicitamente para falar com um 'humano', 'atendente' ou 'analista', sua única resposta deve ser: 'Entendido. Estou encaminhando seu chamado para um analista.'";

            // --- Construção do Payload JSON (Formato OpenAI/Groq) ---
            var messages = new List<GroqMessage>();
            messages.Add(new GroqMessage { Role = "system", Content = systemInstruction });
            messages.Add(new GroqMessage { Role = "system", Content = $"Contexto do Chamado:\nTítulo: {chamado.Titulo}\nDescrição Original: {chamado.Descricao}" });

            foreach (var msg in chamado.Mensagens.OrderBy(m => m.DataEnvio))
            {
                string role = (msg.ClienteRemetenteId.HasValue || msg.FuncionarioRemetenteId.HasValue) ? "user" : "assistant";
                string prefix = msg.FuncionarioRemetenteId.HasValue ? "(Analista): " : "";
                messages.Add(new GroqMessage { Role = role, Content = prefix + msg.Conteudo });
            }

            var requestBody = new GroqRequest { Messages = messages, Model = _modelName, Temperature = 0.7f };

            // --- Chamada HTTP Direta ---
            try
            {
                _logger.LogInformation("Enviando requisição para a API REST da Groq: {ApiUrl}", _apiUrl);
                if (_logger.IsEnabled(LogLevel.Debug)) { string jsonPayload = JsonSerializer.Serialize(requestBody, _jsonOptions); _logger.LogDebug("Payload JSON Groq: {JsonPayload}", jsonPayload); }

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiUrl, requestBody, _jsonOptions);
                _logger.LogInformation("Resposta API Groq recebida com status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var groqResponse = await response.Content.ReadFromJsonAsync<GroqResponse>(_jsonOptions);
                    string? responseText = groqResponse?.Choices?.FirstOrDefault()?.Message?.Content;
                    if (!string.IsNullOrEmpty(responseText))
                    {
                        _logger.LogInformation("SUCESSO: Resposta Groq extraída."); _logger.LogDebug("Texto Resposta Groq: {ResponseText}", responseText);
                        if (responseText.Contains("encaminhando seu chamado")) { _logger.LogInformation("Groq decidiu encaminhar o chamado {ChamadoId}", chamado.Id); }
                        return responseText;
                    }
                    else { _logger.LogWarning("Resposta Groq OK, mas sem texto. ChamadoId: {ChamadoId}", chamado.Id); string rawResponse = await response.Content.ReadAsStringAsync(); _logger.LogDebug($"JSON cru Groq (sem texto): {rawResponse}"); return "(A IA respondeu, mas sem texto.)"; }
                }
                else { string errorContent = await response.Content.ReadAsStringAsync(); _logger.LogError("Erro API Groq ({StatusCode} - {ReasonPhrase}). ChamadoId: {ChamadoId}. Conteúdo: {ErrorContent}", response.StatusCode, response.ReasonPhrase, chamado.Id, errorContent); return $"Erro ao contatar IA ({response.ReasonPhrase}). Ver logs."; }
            }
            catch (Exception ex) { _logger.LogError(ex, "Erro inesperado API Groq. ChamadoId: {ChamadoId}", chamado.Id); return $"Erro inesperado IA: {ex.Message}"; }
        }

        // --- Classes Auxiliares para o JSON da API Groq/OpenAI ---
        private class GroqRequest { [JsonPropertyName("messages")] public List<GroqMessage> Messages { get; set; } = new(); [JsonPropertyName("model")] public string Model { get; set; } = string.Empty; [JsonPropertyName("temperature")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public float Temperature { get; set; } = 0.7f; }
        private class GroqMessage { [JsonPropertyName("role")] public string Role { get; set; } = string.Empty; [JsonPropertyName("content")] public string Content { get; set; } = string.Empty; }
        private class GroqResponse { [JsonPropertyName("choices")] public List<GroqChoice>? Choices { get; set; } /* ... outros ... */ [JsonPropertyName("usage")] public GroqUsage? Usage { get; set; } }
        private class GroqChoice { [JsonPropertyName("message")] public GroqMessage? Message { get; set; } /* ... outros ... */ [JsonPropertyName("finish_reason")] public string? FinishReason { get; set; } }
        private class GroqUsage { [JsonPropertyName("prompt_tokens")] public int PromptTokens { get; set; } [JsonPropertyName("completion_tokens")] public int CompletionTokens { get; set; } [JsonPropertyName("total_tokens")] public int TotalTokens { get; set; } }
    }
}