using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using AgroBusca.Api.Data;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Services;
using Xunit;

namespace AgroBusca.Tests;

public class FichaTecnicaServiceTests
{
    private static AppDbContext CrearContextoEnMemoria(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task InicializarFichasSemillaAsync_DebeCrearFichasOficialesDeTarija()
    {
        using var context = CrearContextoEnMemoria("Test_Fichas_Semilla");
        var service = new FichaTecnicaService(context, NullLogger<FichaTecnicaService>.Instance);

        await service.InicializarFichasSemillaAsync();

        var todas = await service.ObtenerTodasAsync();
        todas.Should().NotBeEmpty();
        todas.Should().Contain(f => f.Titulo.Contains("Clorosis"));
        todas.Should().Contain(f => f.Titulo.Contains("Moscatel"));
    }

    [Fact]
    public async Task ObtenerTodasAsync_ConFiltroPorTipo_DebeFiltrarCorrectamente()
    {
        using var context = CrearContextoEnMemoria("Test_Fichas_Filtro");
        var service = new FichaTecnicaService(context, NullLogger<FichaTecnicaService>.Instance);
        await service.InicializarFichasSemillaAsync();

        var filtro = new FiltroFichaTecnicaDto { Tipo = "Enfermedad" };
        var resultados = await service.ObtenerTodasAsync(filtro);

        resultados.Should().NotBeEmpty();
        resultados.Should().OnlyContain(f => f.Tipo.Equals("Enfermedad", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CrearAsync_Y_EliminarAsync_DebeGestionarCicloDeVida()
    {
        using var context = CrearContextoEnMemoria("Test_Fichas_CRUD");
        var service = new FichaTecnicaService(context, NullLogger<FichaTecnicaService>.Instance);

        var dto = new CrearFichaTecnicaDto
        {
            Titulo = "Poda Guyot Doble en Moscatel de Tarija",
            CultivoVariedad = "Moscatel de Alejandría",
            Tipo = "ManejoAgronomico",
            Zona = "Valle de la Concepción",
            Descripcion = "Poda mixta que deja dos varas de 6-8 yemas y dos pitones de reemplazo.",
            MedidasManejo = "Efectuar en menguante de julio/agosto."
        };

        var creada = await service.CrearAsync(dto);
        creada.Id.Should().BeGreaterThan(0);

        var obtenida = await service.ObtenerPorIdAsync(creada.Id);
        obtenida.Should().NotBeNull();
        obtenida!.Titulo.Should().Be(dto.Titulo);

        var eliminado = await service.EliminarAsync(creada.Id);
        eliminado.Should().BeTrue();

        var inexistente = await service.ObtenerPorIdAsync(creada.Id);
        inexistente.Should().BeNull();
    }
}
