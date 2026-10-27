using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using AgroBusca.Api.Data;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Services;
using Xunit;

namespace AgroBusca.Tests;

public class ParcelaViticolaServiceTests
{
    private static AppDbContext CrearContextoEnMemoria(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CalcularYDisenarParcelaAsync_ParcelaCuadrada_DebeGenerarFilasYPlantasCorrectamente()
    {
        using var context = CrearContextoEnMemoria("Test_Parcela_Calculo");
        var service = new ParcelaViticolaService(context, NullLogger<ParcelaViticolaService>.Instance);

        var lngBase = -64.7300;
        var latBase = -21.5800;
        var deltaLng = 0.000965;
        var deltaLat = 0.000898;

        var request = new DisenoParcelaRequestDto
        {
            Nombre = "Cuartel Experimental Moscatel",
            VariedadVid = "Moscatel de Alejandría",
            Ubicacion = "Valle de la Concepción (Uriondo), Tarija",
            Poligono = new List<CoordenadaDto>
            {
                new(lngBase, latBase),
                new(lngBase + deltaLng, latBase),
                new(lngBase + deltaLng, latBase + deltaLat),
                new(lngBase, latBase + deltaLat)
            },
            DistanciaEntreFilas = 2.5,
            DistanciaEntrePlantas = 1.2,
            OrientacionGrados = 0.0,
            MargenCabeceraMetros = 1.5,
            GuardarEnBaseDatos = true
        };

        var resultado = await service.CalcularYDisenarParcelaAsync(request);

        resultado.Should().NotBeNull();
        resultado.Id.Should().BeGreaterThan(0);
        resultado.Metricas.AreaMetrosCuadrados.Should().BeInRange(9500, 10500);
        resultado.Metricas.TotalFilas.Should().BeInRange(35, 42);
        resultado.Metricas.TotalPlantas.Should().BeGreaterThan(2500);
        resultado.Hileras.Should().NotBeEmpty();
        resultado.Plantas.Should().NotBeEmpty();

        resultado.FilasGeoJson.Should().Contain("FeatureCollection");
        resultado.FilasGeoJson.Should().Contain("LineString");
        resultado.PlantasGeoJson.Should().Contain("FeatureCollection");
        resultado.PlantasGeoJson.Should().Contain("Point");

        foreach (var p in resultado.Plantas)
        {
            p.Lng.Should().BeInRange(lngBase - 0.0001, lngBase + deltaLng + 0.0001);
            p.Lat.Should().BeInRange(latBase - 0.0001, latBase + deltaLat + 0.0001);
        }
    }

    [Fact]
    public async Task CalcularYDisenarParcelaAsync_PoligonoConMenosDeTresPuntos_DebeLanzarExcepcion()
    {
        using var context = CrearContextoEnMemoria("Test_Parcela_Invalida");
        var service = new ParcelaViticolaService(context, NullLogger<ParcelaViticolaService>.Instance);

        var request = new DisenoParcelaRequestDto
        {
            Nombre = "Parcela Inválida",
            Poligono = new List<CoordenadaDto>
            {
                new(-64.73, -21.58),
                new(-64.72, -21.58)
            }
        };

        var act = () => service.CalcularYDisenarParcelaAsync(request);
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
