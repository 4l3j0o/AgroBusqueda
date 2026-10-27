using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using AgroBusca.Api.Data;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Interfaces;
using AgroBusca.Api.Models;
using AgroBusca.Api.Services;
using Xunit;

namespace AgroBusca.Tests;

public class BusquedaServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IEmbeddingService> _mockEmbeddingService;
    private readonly Mock<IRagService> _mockRagService;
    private readonly BusquedaService _service;

    public BusquedaServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"AgroBuscaTests_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        _mockEmbeddingService = new Mock<IEmbeddingService>();
        _mockRagService = new Mock<IRagService>();

        _service = new BusquedaService(
            _context,
            _mockEmbeddingService.Object,
            _mockRagService.Object,
            NullLogger<BusquedaService>.Instance
        );

        SeedTestData();
    }

    private void SeedTestData()
    {
        var doc1 = new Documento
        {
            Id = 1,
            Titulo = "Guía de Clorosis Férrica en Tarija",
            AutorInstitucion = "CENAVIT Tarija",
            Categoria = "Nutrición Vegetal",
            UrlFuente = "https://cenavit.gob.bo/clorosis.pdf",
            FechaPublicacion = DateTime.UtcNow,
            Fragmentos = new List<FragmentoDocumento>
            {
                new()
                {
                    Id = 1,
                    IndiceChunk = 1,
                    Subseccion = "Síntomas",
                    Contenido = "Amarilleamiento intervenal en hojas jóvenes por suelo calcáreo.",
                    Embedding = CrearVectorConValorEnIndice(0, 1.0f)
                }
            }
        };

        var doc2 = new Documento
        {
            Id = 2,
            Titulo = "Manejo de Oídio en Vitis vinifera",
            AutorInstitucion = "INIAF Tarija",
            Categoria = "Sanidad Vegetal",
            UrlFuente = "https://iniaf.gob.bo/oidio.pdf",
            FechaPublicacion = DateTime.UtcNow,
            Fragmentos = new List<FragmentoDocumento>
            {
                new()
                {
                    Id = 2,
                    IndiceChunk = 1,
                    Subseccion = "Identificación",
                    Contenido = "Polvillo blanquecino ceniciento en racimos y hojas.",
                    Embedding = CrearVectorConValorEnIndice(1, 1.0f)
                }
            }
        };

        _context.Documentos.AddRange(doc1, doc2);
        _context.SaveChanges();
    }

    private static float[] CrearVectorConValorEnIndice(int indice, float valor)
    {
        var v = new float[384];
        v[indice] = valor;
        return v;
    }

    [Fact]
    public async Task BuscarSemanticaAsync_DebeRecuperarFragmentosYCalcularSimilitudNormalizada()
    {
        _mockEmbeddingService
            .Setup(e => e.GenerarEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearVectorConValorEnIndice(0, 1.0f));

        var request = new BusquedaRequestDto
        {
            Pregunta = "hojas amarillas",
            TopK = 5,
            UmbralMinimo = 0.1,
            GenerarRespuestaIa = false
        };

        var response = await _service.BuscarSemanticaAsync(request);

        response.Should().NotBeNull();
        response.TotalRecuperados.Should().BeGreaterThan(0);
        response.Fuentes.Should().NotBeEmpty();

        var primeraFuente = response.Fuentes.First();
        primeraFuente.Titulo.Should().Be("Guía de Clorosis Férrica en Tarija");
        primeraFuente.Institucion.Should().Be("CENAVIT Tarija");
        primeraFuente.Similitud.Should().BeInRange(0.0, 1.0);
        primeraFuente.Similitud.Should().BeApproximately(1.0, 0.001);

        response.AvisoAgronomico.Should().NotBeNullOrWhiteSpace();
        response.AvisoAgronomico.Should().Contain("profesional agrónomo");
    }

    [Fact]
    public async Task BuscarSemanticaAsync_DebeFiltrarPorCategoriaCorrectamente()
    {
        _mockEmbeddingService
            .Setup(e => e.GenerarEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearVectorConValorEnIndice(0, 1.0f));

        var request = new BusquedaRequestDto
        {
            Pregunta = "problemas sanitarios",
            Categoria = "Sanidad Vegetal",
            TopK = 5,
            UmbralMinimo = 0.0,
            GenerarRespuestaIa = false
        };

        var response = await _service.BuscarSemanticaAsync(request);

        response.Fuentes.Should().OnlyContain(f => f.Categoria == "Sanidad Vegetal");
    }

    [Fact]
    public async Task BuscarSemanticaAsync_ConGenerarIaTrue_DebeInvocarIRagServiceSinLlamarAGroqReal()
    {
        _mockEmbeddingService
            .Setup(e => e.GenerarEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearVectorConValorEnIndice(0, 1.0f));

        _mockRagService
            .Setup(r => r.GenerarSintesisRagAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<FuenteRespuestaDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("Síntesis técnica simulada para pruebas", "mock-llm-120b", 150));

        var request = new BusquedaRequestDto
        {
            Pregunta = "las hojas de la vid se ponen amarillas",
            TopK = 3,
            UmbralMinimo = 0.1,
            GenerarRespuestaIa = true
        };

        var response = await _service.BuscarSemanticaAsync(request);

        response.Respuesta.Should().Be("Síntesis técnica simulada para pruebas");
        response.ModeloIa.Should().Be("mock-llm-120b");
        _mockRagService.Verify(r => r.GenerarSintesisRagAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<FuenteRespuestaDto>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
