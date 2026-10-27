using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AgroBusca.Api.Data;
using AgroBusca.Api.Interfaces;

namespace AgroBusca.Api.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmbeddingService> _logger;
    private readonly int _dimension;
    private readonly string _proveedor;

    private static readonly Dictionary<string, string[]> GruposSemanticos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["clorosis"] = new[] { "amarillo", "amarilla", "amarillas", "amarillenta", "amarillentas", "amarilleo", "amarilleamiento", "hojas amarillas", "hoja amarilla", "hierro", "deficiencia foliar", "nervaduras verdes", "falta de nutrientes", "clorosis ferrica" },
        ["oidio"] = new[] { "cenicilla", "polvillo blanco", "uncinula necator", "erysiphe necator", "hongo blanco", "manchas cenicientas", "azufre" },
        ["mildiu"] = new[] { "plasmopara viticola", "manchas de aceite", "pelusilla blanca enves", "tizon foliar", "cobre", "caldo bordeles" },
        ["botrytis"] = new[] { "podredumbre gris", "pudricion racimo", "botrytis cinerea", "moho gris", "baya rota" },
        ["riego"] = new[] { "estres hidrico", "riego por goteo", "deficit hidrico", "evapotranspiracion", "humedad del suelo", "frecuencia de riego" },
        ["poda"] = new[] { "poda de formacion", "poda de produccion", "poda verde", "desbrote", "deshoje", "sarmiento", "yemas", "espaldera", "guyot" },
        ["tarija"] = new[] { "valle central", "valle de la concepcion", "uriondo", "calamuchita", "san lorenzo", "bermejo", "vitis vinifera", "altura" },
        ["variedades"] = new[] { "moscatel de alejandria", "singani", "cabernet sauvignon", "syrah", "tannat", "uva de mesa", "red globe" }
    };

    public EmbeddingService(IConfiguration configuration, ILogger<EmbeddingService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _dimension = _configuration.GetValue("Embeddings:Dimension", AppDbContext.DimensionEmbedding);
        _proveedor = _configuration.GetValue("Embeddings:Provider", "LocalDeterministic") ?? "LocalDeterministic";

        _logger.LogInformation("EmbeddingService inicializado. Proveedor: {Proveedor}, Dimensión: {Dimension}", _proveedor, _dimension);
    }

    public int Dimension => _dimension;
    public string Proveedor => _proveedor;

    public Task<float[]> GenerarEmbeddingAsync(string texto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return Task.FromResult(new float[_dimension]);
        }

        var vector = CalcularVectorSemantico(texto, _dimension);
        return Task.FromResult(vector);
    }

    public async Task<IReadOnlyList<float[]>> GenerarEmbeddingsBatchAsync(IReadOnlyList<string> textos, CancellationToken cancellationToken = default)
    {
        var resultados = new List<float[]>(textos.Count);
        foreach (var t in textos)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var vec = await GenerarEmbeddingAsync(t, cancellationToken);
            resultados.Add(vec);
        }
        return resultados;
    }

    private static float[] CalcularVectorSemantico(string texto, int dimension)
    {
        var vector = new float[dimension];
        var textoNormalizado = NormalizarTexto(texto);
        var palabras = textoNormalizado.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (palabras.Length == 0) return vector;

        foreach (var palabra in palabras)
        {
            if (palabra.Length < 2) continue;

            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(palabra));

            for (var i = 0; i < dimension; i++)
            {
                var byteIndex = (i * 3) % hashBytes.Length;
                var val = (hashBytes[byteIndex] / 128f) - 1.0f;
                vector[i] += val;
            }
        }

        foreach (var (conceptoClave, terminosAfines) in GruposSemanticos)
        {
            var pesoConcepto = 0f;

            if (textoNormalizado.Contains(conceptoClave, StringComparison.OrdinalIgnoreCase))
            {
                pesoConcepto += 3.5f;
            }

            foreach (var afin in terminosAfines)
            {
                if (textoNormalizado.Contains(afin, StringComparison.OrdinalIgnoreCase))
                {
                    pesoConcepto += 2.0f;
                }
            }

            if (pesoConcepto > 0f)
            {
                using var sha = SHA256.Create();
                var hashConcepto = sha.ComputeHash(Encoding.UTF8.GetBytes($"concepto_{conceptoClave}"));

                for (var i = 0; i < dimension; i++)
                {
                    var byteIndex = (i * 2) % hashConcepto.Length;
                    var componente = ((hashConcepto[byteIndex] / 128f) - 1.0f) * pesoConcepto;
                    vector[i] += componente;
                }
            }
        }

        var norma = 0.0;
        for (var i = 0; i < dimension; i++)
        {
            norma += vector[i] * vector[i];
        }

        norma = Math.Sqrt(norma);
        if (norma > 1e-9)
        {
            for (var i = 0; i < dimension; i++)
            {
                vector[i] = (float)(vector[i] / norma);
            }
        }

        return vector;
    }

    private static string NormalizarTexto(string texto)
    {
        var sinTildes = texto
            .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
            .Replace("Á", "a").Replace("É", "e").Replace("Í", "i").Replace("Ó", "o").Replace("Ú", "u")
            .Replace("ñ", "n").Replace("Ñ", "n");

        return Regex.Replace(sinTildes.ToLowerInvariant(), @"[^a-z0-9\s]", " ");
    }
}
