using Microsoft.AspNetCore.Mvc;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Interfaces;

namespace AgroBusca.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FichasTecnicasController : ControllerBase
{
    private readonly IFichaTecnicaService _fichaService;
    private readonly ILogger<FichasTecnicasController> _logger;

    public FichasTecnicasController(IFichaTecnicaService fichaService, ILogger<FichasTecnicasController> logger)
    {
        _fichaService = fichaService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FichaTecnicaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FichaTecnicaDto>>> ObtenerTodasAsync(
        [FromQuery] FiltroFichaTecnicaDto filtro,
        CancellationToken cancellationToken)
    {
        var fichas = await _fichaService.ObtenerTodasAsync(filtro, cancellationToken);
        return Ok(fichas);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FichaTecnicaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FichaTecnicaDto>> ObtenerPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var ficha = await _fichaService.ObtenerPorIdAsync(id, cancellationToken);
        if (ficha is null)
        {
            return NotFound(new { Mensaje = $"Ficha técnica con ID {id} no encontrada." });
        }
        return Ok(ficha);
    }

    [HttpPost]
    [ProducesResponseType(typeof(FichaTecnicaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FichaTecnicaDto>> CrearAsync(
        [FromBody] CrearFichaTecnicaDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var creada = await _fichaService.CrearAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorIdAsync), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(FichaTecnicaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FichaTecnicaDto>> ActualizarAsync(
        int id,
        [FromBody] ActualizarFichaTecnicaDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var actualizada = await _fichaService.ActualizarAsync(id, dto, cancellationToken);
        if (actualizada is null)
        {
            return NotFound(new { Mensaje = $"Ficha técnica con ID {id} no encontrada para actualizar." });
        }

        return Ok(actualizada);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarAsync(int id, CancellationToken cancellationToken)
    {
        var eliminado = await _fichaService.EliminarAsync(id, cancellationToken);
        if (!eliminado)
        {
            return NotFound(new { Mensaje = $"Ficha técnica con ID {id} no encontrada." });
        }

        return NoContent();
    }

    [HttpPost("inicializar-semilla")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> InicializarSemillaAsync(CancellationToken cancellationToken)
    {
        await _fichaService.InicializarFichasSemillaAsync(cancellationToken);
        var total = (await _fichaService.ObtenerTodasAsync(null, cancellationToken)).Count;
        return Ok(new { Mensaje = "Fichas técnicas vitícolas semilla inicializadas.", Total = total });
    }
}
