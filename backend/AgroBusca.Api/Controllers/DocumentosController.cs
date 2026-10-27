using Microsoft.AspNetCore.Mvc;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Interfaces;

namespace AgroBusca.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DocumentosController : ControllerBase
{
    private readonly IDocumentoService _documentoService;
    private readonly ILogger<DocumentosController> _logger;

    public DocumentosController(
        IDocumentoService documentoService,
        ILogger<DocumentosController> logger)
    {
        _documentoService = documentoService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentoResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DocumentoResponseDto>>> ObtenerTodosAsync(CancellationToken cancellationToken)
    {
        var documentos = await _documentoService.ObtenerTodosAsync(cancellationToken);
        return Ok(documentos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DocumentoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentoResponseDto>> ObtenerPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var documento = await _documentoService.ObtenerPorIdAsync(id, cancellationToken);
        if (documento is null)
        {
            return NotFound(new { Mensaje = $"No se encontró ningún documento con ID {id}." });
        }

        return Ok(documento);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DocumentoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentoResponseDto>> CrearAsync(
        [FromBody] DocumentoCreateDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var creado = await _documentoService.CrearDocumentoConFragmentacionAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorIdAsync), new { id = creado.Id }, creado);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarAsync(int id, CancellationToken cancellationToken)
    {
        var eliminado = await _documentoService.EliminarDocumentoAsync(id, cancellationToken);
        if (!eliminado)
        {
            return NotFound(new { Mensaje = $"No se encontró ningún documento con ID {id}." });
        }

        return NoContent();
    }

    [HttpGet("estadisticas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ObtenerEstadisticasAsync(CancellationToken cancellationToken)
    {
        var totalDocs = await _documentoService.ObtenerTotalDocumentosAsync(cancellationToken);
        var totalChunks = await _documentoService.ObtenerTotalFragmentosAsync(cancellationToken);

        return Ok(new
        {
            TotalDocumentos = totalDocs,
            TotalFragmentosVectorizados = totalChunks,
            ZonaGeografica = "Valle Central de Tarija, Bolivia",
            RubroPrincipal = "Viticultura y Enología (Uva de mesa, Vinos y Singani)"
        });
    }

    [HttpPost("inicializar-semilla")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> InicializarSemillaAsync(CancellationToken cancellationToken)
    {
        await _documentoService.InicializarDatosSemillaAsync(cancellationToken);

        var totalDocs = await _documentoService.ObtenerTotalDocumentosAsync(cancellationToken);
        var totalChunks = await _documentoService.ObtenerTotalFragmentosAsync(cancellationToken);

        return Ok(new
        {
            Mensaje = "Inicialización de datos semilla de viticultura completada.",
            TotalDocumentos = totalDocs,
            TotalFragmentosIndexados = totalChunks
        });
    }
}
