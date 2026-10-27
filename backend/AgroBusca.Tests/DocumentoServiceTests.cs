using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using AgroBusca.Api.Data;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Interfaces;
using AgroBusca.Api.Services;
using Xunit;

namespace AgroBusca.Tests;

public class DocumentoServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IEmbeddingService> _mockEmbeddingService;
    private readonly DocumentoService _service;

    public DocumentoServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"AgroBuscaDocTests_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        _mockEmbeddingService = new Mock<IEmbeddingService>();

        _mockEmbeddingService
            .Setup(e => e.GenerarEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new float[384]);

        _service = new DocumentoService(
            _context,
            _mockEmbeddingService.Object,
            NullLogger<DocumentoService>.Instance
        );
    }

    [Fact]
    public async Task CrearDocumentoConFragmentacionAsync_DebeFragmentarTextoYPersistirVectores()
    {
        var dto = new DocumentoCreateDto
        {
            Titulo = "Manual de Riego en Viñedos de Uriondo",
            AutorInstitucion = "CENAVIT Tarija",
            Categoria = "Riego y Fertirriego",
            UrlFuente = "https://cenavit.gob.bo/riego.pdf",
            FechaPublicacion = DateTime.UtcNow,
            Subseccion = "Técnica",
            ContenidoCompleto = 
                "El riego por goteo permite controlar con exactitud los aportes hídricos en épocas de estrés estival.\n\n" +
                "La frecuencia de aplicación debe adaptarse a la textura del suelo y a la tasa de evapotranspiración diaria del viñedo."
        };

        var resultado = await _service.CrearDocumentoConFragmentacionAsync(dto);

        resultado.Should().NotBeNull();
        resultado.Titulo.Should().Be("Manual de Riego en Viñedos de Uriondo");
        resultado.CantidadFragmentos.Should().Be(2);

        var totalDocs = await _service.ObtenerTotalDocumentosAsync();
        var totalChunks = await _service.ObtenerTotalFragmentosAsync();

        totalDocs.Should().Be(1);
        totalChunks.Should().Be(2);
    }

    [Fact]
    public async Task InicializarDatosSemillaAsync_SiYaExistenDocumentos_NoDebeDuplicar()
    {
        await _service.InicializarDatosSemillaAsync();
        var primerConteo = await _service.ObtenerTotalDocumentosAsync();

        await _service.InicializarDatosSemillaAsync();
        var segundoConteo = await _service.ObtenerTotalDocumentosAsync();

        primerConteo.Should().Be(5);
        segundoConteo.Should().Be(5);
    }
}
