using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using AgroBusca.Api.Services;
using Xunit;

namespace AgroBusca.Tests;

public class EmbeddingServiceTests
{
    private readonly EmbeddingService _service;

    public EmbeddingServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Embeddings:Dimension", "384"},
            {"Embeddings:Provider", "LocalDeterministic"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _service = new EmbeddingService(configuration, NullLogger<EmbeddingService>.Instance);
    }

    [Fact]
    public async Task GenerarEmbeddingAsync_DebeTenerExactamente384Dimensiones()
    {
        var vector = await _service.GenerarEmbeddingAsync("mildiu en vides de Tarija");

        vector.Should().NotBeNull();
        vector.Length.Should().Be(384);
    }

    [Fact]
    public async Task GenerarEmbeddingAsync_DebeEstarNormalizadoL2()
    {
        var vector = await _service.GenerarEmbeddingAsync("clorosis ferrica en suelos calcareos");

        double norma = 0.0;
        foreach (var v in vector)
        {
            norma += v * v;
        }

        norma = Math.Sqrt(norma);
        norma.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public async Task GenerarEmbeddingAsync_DebeSerDeterminista()
    {
        var vector1 = await _service.GenerarEmbeddingAsync("poda en espaldera para Moscatel");
        var vector2 = await _service.GenerarEmbeddingAsync("poda en espaldera para Moscatel");

        vector1.Should().Equal(vector2);
    }

    [Fact]
    public async Task GenerarEmbeddingAsync_DebeMostrarMayorAfinidadEntreTerminosSinonimosQueDivergentes()
    {
        var vHojasAmarillas = await _service.GenerarEmbeddingAsync("las hojas de la vid se ponen amarillas");
        var vClorosis = await _service.GenerarEmbeddingAsync("diagnostico de clorosis ferrica y falta de nutrientes");
        var vRiego = await _service.GenerarEmbeddingAsync("estrategias de riego por goteo y caudal");

        double simConClorosis = CalcularDotProduct(vHojasAmarillas, vClorosis);
        double simConRiego = CalcularDotProduct(vHojasAmarillas, vRiego);

        simConClorosis.Should().BeGreaterThan(simConRiego);
    }

    private static double CalcularDotProduct(float[] a, float[] b)
    {
        double dot = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
        }
        return dot;
    }
}
