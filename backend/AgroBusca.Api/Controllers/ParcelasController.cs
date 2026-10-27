using Microsoft.AspNetCore.Mvc;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Interfaces;

namespace AgroBusca.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ParcelasController : ControllerBase
{
    private readonly IParcelaViticolaService _parcelaService;
    private readonly ILogger<ParcelasController> _logger;

    public ParcelasController(IParcelaViticolaService parcelaService, ILogger<ParcelasController> logger)
    {
        _parcelaService = parcelaService;
        _logger = logger;
    }

    [HttpPost("disenar")]
    [ProducesResponseType(typeof(DisenoParcelaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DisenoParcelaResponseDto>> DisenarAsync(
        [FromBody] DisenoParcelaRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var resultado = await _parcelaService.CalcularYDisenarParcelaAsync(request, cancellationToken);
            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Mensaje = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ParcelaResumenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ParcelaResumenDto>>> ObtenerTodasAsync(CancellationToken cancellationToken)
    {
        var parcelas = await _parcelaService.ObtenerTodasAsync(cancellationToken);
        return Ok(parcelas);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DisenoParcelaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisenoParcelaResponseDto>> ObtenerPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var parcela = await _parcelaService.ObtenerPorIdAsync(id, cancellationToken);
        if (parcela is null)
        {
            return NotFound(new { Mensaje = $"Parcela vitícola con ID {id} no encontrada." });
        }
        return Ok(parcela);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarAsync(int id, CancellationToken cancellationToken)
    {
        var eliminado = await _parcelaService.EliminarAsync(id, cancellationToken);
        if (!eliminado)
        {
            return NotFound(new { Mensaje = $"Parcela vitícola con ID {id} no encontrada." });
        }
        return NoContent();
    }
}
