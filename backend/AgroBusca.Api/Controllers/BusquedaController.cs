using Microsoft.AspNetCore.Mvc;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Interfaces;

namespace AgroBusca.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BusquedaController : ControllerBase
{
    private readonly IBusquedaService _busquedaService;
    private readonly ILogger<BusquedaController> _logger;

    public BusquedaController(
        IBusquedaService busquedaService,
        ILogger<BusquedaController> logger)
    {
        _busquedaService = busquedaService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BusquedaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BusquedaResponseDto>> BuscarAsync(
        [FromBody] BusquedaRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultado = await _busquedaService.BuscarSemanticaAsync(request, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("categorias")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> ObtenerCategoriasAsync(CancellationToken cancellationToken)
    {
        var categorias = await _busquedaService.ObtenerCategoriasAsync(cancellationToken);
        return Ok(categorias);
    }

    [HttpGet("sugerencias")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<string>> ObtenerSugerencias()
    {
        var sugerencias = new List<string>
        {
            "las hojas de la vid se ponen amarillas",
            "polvillo blanco ceniciento en las hojas y racimos",
            "manchas de aceite en las hojas después de lluvias",
            "riego por goteo y estrés hídrico en época de maduración",
            "poda en espaldera para uva Moscatel de Alejandría"
        };

        return Ok(sugerencias);
    }
}
