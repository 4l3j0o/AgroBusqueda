using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AgroBusca.Api.Data;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Interfaces;
using AgroBusca.Api.Models;

namespace AgroBusca.Api.Services;

public class ParcelaViticolaService : IParcelaViticolaService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ParcelaViticolaService> _logger;

    public ParcelaViticolaService(AppDbContext context, ILogger<ParcelaViticolaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DisenoParcelaResponseDto> CalcularYDisenarParcelaAsync(DisenoParcelaRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Poligono == null || request.Poligono.Count < 3)
        {
            throw new ArgumentException("El polígono de la parcela debe contener al menos 3 vértices.", nameof(request));
        }

        var lat0 = request.Poligono.Average(p => p.Lat);
        var lng0 = request.Poligono.Average(p => p.Lng);

        var phi0 = lat0 * Math.PI / 180.0;
        var metersPerLat = 111320.0;
        var metersPerLng = 111320.0 * Math.Cos(phi0);

        var verticesMetricos = request.Poligono.Select(p => new Punto2D(
            (p.Lng - lng0) * metersPerLng,
            (p.Lat - lat0) * metersPerLat
        )).ToList();

        var areaM2 = Math.Abs(CalcularAreaPoligono(verticesMetricos));
        var areaHa = areaM2 / 10000.0;

        var alpha = request.OrientacionGrados * Math.PI / 180.0;
        var cosAlpha = Math.Cos(alpha);
        var sinAlpha = Math.Sin(alpha);

        var rotados = verticesMetricos.Select(v => new Punto2D(
            v.X * cosAlpha + v.Y * sinAlpha,
            -v.X * sinAlpha + v.Y * cosAlpha
        )).ToList();

        var minY = rotados.Min(p => p.Y);
        var maxY = rotados.Max(p => p.Y);

        var hileras = new List<HileraViticolaDto>();
        var plantas = new List<PlantaViticolaDto>();
        var totalMetrosLineales = 0.0;

        var distanciaFilas = Math.Max(1.0, request.DistanciaEntreFilas);
        var distanciaPlantas = Math.Max(0.5, request.DistanciaEntrePlantas);
        var margen = Math.Max(0.0, request.MargenCabeceraMetros);

        var hileraIdx = 1;
        var currentY = minY + margen + (distanciaFilas / 2.0);

        while (currentY <= (maxY - margen))
        {
            var interseccionesX = new List<double>();
            var n = rotados.Count;

            for (var i = 0; i < n; i++)
            {
                var p1 = rotados[i];
                var p2 = rotados[(i + 1) % n];

                if ((p1.Y <= currentY && p2.Y > currentY) || (p2.Y <= currentY && p1.Y > currentY))
                {
                    var t = (currentY - p1.Y) / (p2.Y - p1.Y);
                    var xInt = p1.X + t * (p2.X - p1.X);
                    interseccionesX.Add(xInt);
                }
            }

            interseccionesX.Sort();

            for (var j = 0; j < interseccionesX.Count - 1; j += 2)
            {
                var xStart = interseccionesX[j] + margen;
                var xEnd = interseccionesX[j + 1] - margen;

                var longitudHilera = xEnd - xStart;
                if (longitudHilera >= distanciaPlantas)
                {
                    totalMetrosLineales += longitudHilera;

                    var startRot = new Punto2D(xStart, currentY);
                    var endRot = new Punto2D(xEnd, currentY);

                    var startMet = Desrotar(startRot, cosAlpha, sinAlpha);
                    var endMet = Desrotar(endRot, cosAlpha, sinAlpha);

                    var coordStart = new CoordenadaDto(
                        lng0 + startMet.X / metersPerLng,
                        lat0 + startMet.Y / metersPerLat
                    );

                    var coordEnd = new CoordenadaDto(
                        lng0 + endMet.X / metersPerLng,
                        lat0 + endMet.Y / metersPerLat
                    );

                    var plantasEnHilera = 0;
                    var plantX = xStart + (distanciaPlantas / 2.0);

                    while (plantX <= (xEnd - (distanciaPlantas / 2.0)))
                    {
                        plantasEnHilera++;
                        var pRot = new Punto2D(plantX, currentY);
                        var pMet = Desrotar(pRot, cosAlpha, sinAlpha);

                        plantas.Add(new PlantaViticolaDto
                        {
                            HileraIndice = hileraIdx,
                            PlantaIndice = plantasEnHilera,
                            Lng = lng0 + pMet.X / metersPerLng,
                            Lat = lat0 + pMet.Y / metersPerLat
                        });

                        plantX += distanciaPlantas;
                    }

                    hileras.Add(new HileraViticolaDto
                    {
                        Indice = hileraIdx,
                        LongitudMetros = Math.Round(longitudHilera, 2),
                        CantidadPlantas = plantasEnHilera,
                        Segmento = new List<CoordenadaDto> { coordStart, coordEnd }
                    });

                    hileraIdx++;
                }
            }

            currentY += distanciaFilas;
        }

        var metricas = new MetricasViticolaDto
        {
            AreaMetrosCuadrados = Math.Round(areaM2, 2),
            AreaHectareas = Math.Round(areaHa, 4),
            TotalFilas = hileras.Count,
            TotalPlantas = plantas.Count,
            DensidadPlantasPorHectarea = areaHa > 0 ? Math.Round(plantas.Count / areaHa, 0) : 0,
            LongitudLinealTotalMetros = Math.Round(totalMetrosLineales, 2),
            AlambreEspalderaEstimadoMetros = Math.Round(totalMetrosLineales * 3.5, 1),
            PostesIntermediosEstimados = (int)Math.Ceiling(totalMetrosLineales / 5.5),
            PostesCabecerosEstimados = hileras.Count * 2
        };

        var poligonoGeoJson = ConstruirPoligonoGeoJson(request.Poligono);
        var filasGeoJson = ConstruirFilasGeoJson(hileras);
        var plantasGeoJson = ConstruirPlantasGeoJson(plantas);

        var response = new DisenoParcelaResponseDto
        {
            Nombre = request.Nombre,
            VariedadVid = request.VariedadVid,
            Ubicacion = request.Ubicacion,
            DistanciaEntreFilas = distanciaFilas,
            DistanciaEntrePlantas = distanciaPlantas,
            OrientacionGrados = request.OrientacionGrados,
            MargenCabeceraMetros = margen,
            Metricas = metricas,
            Hileras = hileras,
            Plantas = plantas,
            PoligonoGeoJson = poligonoGeoJson,
            FilasGeoJson = filasGeoJson,
            PlantasGeoJson = plantasGeoJson,
            FechaCreacion = DateTime.UtcNow
        };

        if (request.GuardarEnBaseDatos)
        {
            var entidad = new ParcelaViticola
            {
                Nombre = request.Nombre,
                VariedadVid = request.VariedadVid,
                Ubicacion = request.Ubicacion,
                PoligonoGeoJson = poligonoGeoJson,
                DistanciaEntreFilas = distanciaFilas,
                DistanciaEntrePlantas = distanciaPlantas,
                OrientacionGrados = request.OrientacionGrados,
                MargenCabeceraMetros = margen,
                AreaMetrosCuadrados = metricas.AreaMetrosCuadrados,
                TotalFilas = metricas.TotalFilas,
                TotalPlantas = metricas.TotalPlantas,
                MetrosLinealesTotales = metricas.LongitudLinealTotalMetros,
                FilasGeoJson = filasGeoJson,
                PlantasGeoJson = plantasGeoJson,
                FechaCreacion = DateTime.UtcNow
            };

            _context.ParcelasViticolas.Add(entidad);
            await _context.SaveChangesAsync(cancellationToken);

            response.Id = entidad.Id;
            _logger.LogInformation("Parcela vitícola '{Nombre}' diseñada y guardada con ID {Id}. Total filas: {Filas}, Plantas: {Plantas}",
                entidad.Nombre, entidad.Id, entidad.TotalFilas, entidad.TotalPlantas);
        }

        return response;
    }

    public async Task<IReadOnlyList<ParcelaResumenDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
    {
        var parcelas = await _context.ParcelasViticolas
            .AsNoTracking()
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => new ParcelaResumenDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                VariedadVid = p.VariedadVid,
                Ubicacion = p.Ubicacion,
                AreaMetrosCuadrados = p.AreaMetrosCuadrados,
                AreaHectareas = Math.Round(p.AreaMetrosCuadrados / 10000.0, 4),
                TotalFilas = p.TotalFilas,
                TotalPlantas = p.TotalPlantas,
                DistanciaEntreFilas = p.DistanciaEntreFilas,
                DistanciaEntrePlantas = p.DistanciaEntrePlantas,
                OrientacionGrados = p.OrientacionGrados,
                FechaCreacion = p.FechaCreacion
            })
            .ToListAsync(cancellationToken);

        return parcelas;
    }

    public async Task<DisenoParcelaResponseDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entidad = await _context.ParcelasViticolas
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (entidad is null) return null;

        var areaHa = Math.Round(entidad.AreaMetrosCuadrados / 10000.0, 4);

        return new DisenoParcelaResponseDto
        {
            Id = entidad.Id,
            Nombre = entidad.Nombre,
            VariedadVid = entidad.VariedadVid,
            Ubicacion = entidad.Ubicacion,
            DistanciaEntreFilas = entidad.DistanciaEntreFilas,
            DistanciaEntrePlantas = entidad.DistanciaEntrePlantas,
            OrientacionGrados = entidad.OrientacionGrados,
            MargenCabeceraMetros = entidad.MargenCabeceraMetros,
            PoligonoGeoJson = entidad.PoligonoGeoJson,
            FilasGeoJson = entidad.FilasGeoJson,
            PlantasGeoJson = entidad.PlantasGeoJson,
            FechaCreacion = entidad.FechaCreacion,
            Metricas = new MetricasViticolaDto
            {
                AreaMetrosCuadrados = entidad.AreaMetrosCuadrados,
                AreaHectareas = areaHa,
                TotalFilas = entidad.TotalFilas,
                TotalPlantas = entidad.TotalPlantas,
                DensidadPlantasPorHectarea = areaHa > 0 ? Math.Round(entidad.TotalPlantas / areaHa, 0) : 0,
                LongitudLinealTotalMetros = entidad.MetrosLinealesTotales,
                AlambreEspalderaEstimadoMetros = Math.Round(entidad.MetrosLinealesTotales * 3.5, 1),
                PostesIntermediosEstimados = (int)Math.Ceiling(entidad.MetrosLinealesTotales / 5.5),
                PostesCabecerosEstimados = entidad.TotalFilas * 2
            }
        };
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var entidad = await _context.ParcelasViticolas.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (entidad is null) return false;

        _context.ParcelasViticolas.Remove(entidad);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Parcela vitícola ID {Id} eliminada.", id);
        return true;
    }

    private static Punto2D Desrotar(Punto2D p, double cosAlpha, double sinAlpha)
    {
        return new Punto2D(
            p.X * cosAlpha - p.Y * sinAlpha,
            p.X * sinAlpha + p.Y * cosAlpha
        );
    }

    private static double CalcularAreaPoligono(List<Punto2D> puntos)
    {
        var area = 0.0;
        var n = puntos.Count;
        for (var i = 0; i < n; i++)
        {
            var p1 = puntos[i];
            var p2 = puntos[(i + 1) % n];
            area += (p1.X * p2.Y) - (p2.X * p1.Y);
        }
        return area / 2.0;
    }

    private static string ConstruirPoligonoGeoJson(List<CoordenadaDto> vertices)
    {
        var coords = vertices.Select(v => new[] { v.Lng, v.Lat }).ToList();
        if (coords.Count > 0 && (coords[0][0] != coords[^1][0] || coords[0][1] != coords[^1][1]))
        {
            coords.Add(new[] { coords[0][0], coords[0][1] });
        }

        var feature = new
        {
            type = "Feature",
            geometry = new
            {
                type = "Polygon",
                coordinates = new[] { coords }
            },
            properties = new { tipo = "PerimetroParcela" }
        };

        return JsonSerializer.Serialize(feature);
    }

    private static string ConstruirFilasGeoJson(List<HileraViticolaDto> hileras)
    {
        var features = hileras.Select(h => new
        {
            type = "Feature",
            geometry = new
            {
                type = "LineString",
                coordinates = h.Segmento.Select(s => new[] { s.Lng, s.Lat }).ToArray()
            },
            properties = new
            {
                hilera = h.Indice,
                longitudMetros = h.LongitudMetros,
                plantas = h.CantidadPlantas
            }
        });

        var collection = new
        {
            type = "FeatureCollection",
            features
        };

        return JsonSerializer.Serialize(collection);
    }

    private static string ConstruirPlantasGeoJson(List<PlantaViticolaDto> plantas)
    {
        var features = plantas.Select(p => new
        {
            type = "Feature",
            geometry = new
            {
                type = "Point",
                coordinates = new[] { p.Lng, p.Lat }
            },
            properties = new
            {
                hilera = p.HileraIndice,
                planta = p.PlantaIndice
            }
        });

        var collection = new
        {
            type = "FeatureCollection",
            features
        };

        return JsonSerializer.Serialize(collection);
    }

    private readonly record struct Punto2D(double X, double Y);
}
