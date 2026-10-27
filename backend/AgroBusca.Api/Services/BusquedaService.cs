using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using AgroBusca.Api.Data;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Interfaces;
using AgroBusca.Api.Mappings;

namespace AgroBusca.Api.Services;

public class BusquedaService : IBusquedaService
{
    private readonly AppDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly IRagService _ragService;
    private readonly ILogger<BusquedaService> _logger;

    public BusquedaService(
        AppDbContext context,
        IEmbeddingService embeddingService,
        IRagService ragService,
        ILogger<BusquedaService> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _ragService = ragService;
        _logger = logger;
    }

    public async Task<BusquedaResponseDto> BuscarSemanticaAsync(BusquedaRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Iniciando búsqueda semántica para: '{Pregunta}'. TopK: {TopK}, Categoria: {Categoria}, IA (RAG): {Ia}",
            request.Pregunta, request.TopK, request.Categoria ?? "Todas", request.GenerarRespuestaIa);

        var queryVector = await _embeddingService.GenerarEmbeddingAsync(request.Pregunta, cancellationToken);

        var query = _context.FragmentosDocumento
            .Include(f => f.Documento)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Categoria))
        {
            var categoriaNormalizada = request.Categoria.Trim();
            query = query.Where(f => f.Documento.Categoria.ToLower() == categoriaNormalizada.ToLower());
        }

        var fragmentos = await query.ToListAsync(cancellationToken);

        var resultadosConScore = fragmentos
            .Select(f => new
            {
                Fragmento = f,
                Similitud = CalcularSimilitudCoseno(queryVector, f.Embedding)
            })
            .Where(x => x.Similitud >= request.UmbralMinimo)
            .OrderByDescending(x => x.Similitud)
            .Take(request.TopK)
            .Select(x => x.Fragmento.ToFuenteRespuestaDto(x.Similitud))
            .ToList();

        string? sintesisIa = null;
        string? modeloIa = null;
        long tiempoIaMs = 0;

        if (request.GenerarRespuestaIa && resultadosConScore.Count > 0)
        {
            _logger.LogInformation("Generando síntesis RAG con Groq para {Count} fragmentos...", resultadosConScore.Count);
            var ragResult = await _ragService.GenerarSintesisRagAsync(request.Pregunta, resultadosConScore, cancellationToken);
            sintesisIa = ragResult.Sintesis;
            modeloIa = ragResult.Modelo;
            tiempoIaMs = ragResult.TiempoMs;
        }

        stopwatch.Stop();

        _logger.LogInformation("Búsqueda semántica finalizada en {Tiempo} ms (RAG IA: {TiempoIa} ms). Resultados: {Total}",
            stopwatch.ElapsedMilliseconds, tiempoIaMs, resultadosConScore.Count);

        return new BusquedaResponseDto
        {
            Consulta = request.Pregunta,
            TotalRecuperados = resultadosConScore.Count,
            TiempoRespuestaMs = stopwatch.ElapsedMilliseconds,
            Respuesta = sintesisIa,
            ModeloIa = modeloIa,
            TiempoIaMs = tiempoIaMs,
            Fuentes = resultadosConScore
        };
    }

    public async Task<IReadOnlyList<string>> ObtenerCategoriasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documentos
            .AsNoTracking()
            .Select(d => d.Categoria)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);
    }

    private static double CalcularSimilitudCoseno(float[] v1, float[] v2)
    {
        if (v1.Length == 0 || v2.Length == 0 || v1.Length != v2.Length)
        {
            return 0.0;
        }

        double dot = 0.0;
        for (var i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
        }

        return Math.Clamp(dot, 0.0, 1.0);
    }
}
