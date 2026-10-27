using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Interfaces;

namespace AgroBusca.Api.Services;

public class GroqRagService : IRagService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GroqRagService> _logger;

    private readonly string _apiKey;
    private readonly string _primaryModel;
    private readonly string _fallbackModel;
    private readonly string _endpoint;

    public GroqRagService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GroqRagService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _apiKey = _configuration["Groq:ApiKey"] ?? string.Empty;
        _primaryModel = _configuration["Groq:Model"] ?? "qwen/qwen3.8-27b";
        _fallbackModel = _configuration["Groq:FallbackModel"] ?? "openai/gpt-oss-20b";
        _endpoint = _configuration["Groq:Endpoint"] ?? "https://api.groq.com/openai/v1/chat/completions";
    }

    public async Task<(string? Sintesis, string? Modelo, long TiempoMs)> GenerarSintesisRagAsync(
        string pregunta,
        IReadOnlyList<FuenteRespuestaDto> fragmentos,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || fragmentos.Count == 0)
        {
            return (null, null, 0);
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var fragmentosTop = fragmentos.Take(3).ToList();

            var contextoBuilder = new StringBuilder();
            for (var i = 0; i < fragmentosTop.Count; i++)
            {
                var f = fragmentosTop[i];
                contextoBuilder.AppendLine($"--- [DOCUMENTO {i + 1}] ---");
                contextoBuilder.AppendLine($"Título: {f.TituloDocumento}");
                contextoBuilder.AppendLine($"Institución / Autor: {f.AutorInstitucion}");
                contextoBuilder.AppendLine($"Categoría: {f.Categoria}");
                if (!string.IsNullOrWhiteSpace(f.Subseccion))
                {
                    contextoBuilder.AppendLine($"Sección: {f.Subseccion}");
                }
                contextoBuilder.AppendLine($"Texto técnico: {f.Contenido}");
                contextoBuilder.AppendLine();
            }

            var systemPrompt = 
                "Eres AgroBusca IA, asistente técnico agronómico especializado en viticultura y producción de uva " +
                "del Valle Central de Tarija, Bolivia (Uriondo, Calamuchita, Santa Ana, San Lorenzo, etc.).\n\n" +
                "Tu objetivo es brindar una síntesis técnica, clara y fundamentada a la pregunta del productor o técnico, " +
                "basándote EXCLUSIVAMENTE en los fragmentos de documentos técnicos provistos en el contexto.\n\n" +
                "Reglas obligatorias:\n" +
                "1. Cita explícitamente a las instituciones y fuentes de donde proviene cada recomendación (ej: 'Según la guía técnica del CENAVIT...', 'De acuerdo con el INIAF Tarija...').\n" +
                "2. Si la información provista en los fragmentos no responde completamente a la pregunta, indícalo de forma profesional sin inventar ni alucinar.\n" +
                "3. Mantén un tono técnico pero accesible para agricultores y enólogos.\n" +
                "4. Responde en español con formato Markdown limpio (párrafos breves y viñetas para pasos o recomendaciones).\n" +
                "5. Concluye recordando que se debe verificar en campo con un agrónomo calificado.";

            var userPrompt = 
                $"Pregunta del usuario: \"{pregunta}\"\n\n" +
                $"Fragmentos técnicos recuperados de la base de datos:\n\n{contextoBuilder}\n\n" +
                "Genera la síntesis técnica estructurada basándote en estos datos:";

            var resultado = await EnviarPeticionGroqAsync(_primaryModel, systemPrompt, userPrompt, cancellationToken);

            if (resultado is null && !string.IsNullOrWhiteSpace(_fallbackModel))
            {
                _logger.LogWarning("Reintentando llamada a Groq con el modelo de respaldo: {FallbackModel}", _fallbackModel);
                resultado = await EnviarPeticionGroqAsync(_fallbackModel, systemPrompt, userPrompt, cancellationToken);
            }

            stopwatch.Stop();

            if (resultado is not null)
            {
                return (resultado.Value.Contenido, resultado.Value.Modelo, stopwatch.ElapsedMilliseconds);
            }

            return (null, null, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error al generar síntesis RAG con la API de Groq: {Message}", ex.Message);
            return (null, null, stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task<(string Contenido, string Modelo)?> EnviarPeticionGroqAsync(
        string modelo,
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var payload = new
        {
            model = modelo,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.1,
            max_tokens = 450
        };

        var json = JsonSerializer.Serialize(payload);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Groq API error ({StatusCode}) con modelo {Modelo}: {Body}", response.StatusCode, modelo, errorBody);
            return null;
        }

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseJson);

        var choices = doc.RootElement.GetProperty("choices");
        if (choices.GetArrayLength() > 0)
        {
            var message = choices[0].GetProperty("message");
            var content = message.GetProperty("content").GetString();

            if (!string.IsNullOrWhiteSpace(content))
            {
                return (content.Trim(), modelo);
            }
        }

        return null;
    }
}
