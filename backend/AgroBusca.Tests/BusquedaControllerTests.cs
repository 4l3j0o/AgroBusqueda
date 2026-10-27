using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using AgroBusca.Api.Controllers;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Interfaces;
using Xunit;

namespace AgroBusca.Tests;

public class BusquedaControllerTests
{
    private readonly Mock<IBusquedaService> _mockBusquedaService;
    private readonly BusquedaController _controller;

    public BusquedaControllerTests()
    {
        _mockBusquedaService = new Mock<IBusquedaService>();
        _controller = new BusquedaController(_mockBusquedaService.Object, NullLogger<BusquedaController>.Instance);
    }

    [Fact]
    public async Task BuscarAsync_ConDatosValidos_DebeRetornarOkConBusquedaResponseDto()
    {
        var request = new BusquedaRequestDto
        {
            Pregunta = "clorosis en vid",
            TopK = 5
        };

        var expectedResponse = new BusquedaResponseDto
        {
            Consulta = "clorosis en vid",
            TotalRecuperados = 1,
            Fuentes = new List<FuenteRespuestaDto>
            {
                new()
                {
                    DocumentoId = 1,
                    Titulo = "Guía de Clorosis",
                    Institucion = "CENAVIT",
                    Similitud = 0.85
                }
            }
        };

        _mockBusquedaService
            .Setup(s => s.BuscarSemanticaAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.BuscarAsync(request, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var responseDto = okResult.Value as BusquedaResponseDto;
        responseDto.Should().NotBeNull();
        responseDto!.Consulta.Should().Be("clorosis en vid");
        responseDto.Fuentes.Should().HaveCount(1);
    }

    [Fact]
    public async Task BuscarAsync_ConModelStateInvalido_DebeRetornarBadRequest()
    {
        _controller.ModelState.AddModelError("Pregunta", "La consulta es obligatoria.");
        var request = new BusquedaRequestDto { Pregunta = "" };

        var result = await _controller.BuscarAsync(request, CancellationToken.None);

        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }

    [Fact]
    public void ObtenerSugerencias_DebeRetornarConsultasAgronomicasDeTarija()
    {
        var result = _controller.ObtenerSugerencias();

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var sugerencias = okResult.Value as IReadOnlyList<string>;
        sugerencias.Should().NotBeNull();
        sugerencias.Should().NotBeEmpty();
        sugerencias.Should().Contain(s => s.Contains("Moscatel de Alejandría"));
    }
}
