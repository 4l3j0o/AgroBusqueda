using System.ComponentModel.DataAnnotations;

namespace AgroBusca.Api.DTOs;

public class CoordenadaDto
{
    public double Lng { get; set; }
    public double Lat { get; set; }

    public CoordenadaDto() { }

    public CoordenadaDto(double lng, double lat)
    {
        Lng = lng;
        Lat = lat;
    }
}

public class DisenoParcelaRequestDto
{
    [Required(ErrorMessage = "El nombre de la parcela o cuartel es obligatorio.")]
    [StringLength(200)]
    public string Nombre { get; set; } = "Cuartel Vitícola";

    [Required(ErrorMessage = "La variedad de vid es obligatoria.")]
    [StringLength(150)]
    public string VariedadVid { get; set; } = "Moscatel de Alejandría";

    [Required(ErrorMessage = "La ubicación o valle es obligatoria.")]
    [StringLength(200)]
    public string Ubicacion { get; set; } = "Valle Central de Tarija (Uriondo)";

    [Required(ErrorMessage = "El polígono de la parcela es obligatorio.")]
    [MinLength(3, ErrorMessage = "El polígono debe tener al menos 3 vértices.")]
    public List<CoordenadaDto> Poligono { get; set; } = new();

    [Range(1.0, 5.0, ErrorMessage = "La distancia entre filas debe estar entre 1.0 m y 5.0 m.")]
    public double DistanciaEntreFilas { get; set; } = 2.5;

    [Range(0.5, 3.0, ErrorMessage = "La distancia entre plantas debe estar entre 0.5 m y 3.0 m.")]
    public double DistanciaEntrePlantas { get; set; } = 1.2;

    [Range(0.0, 360.0, ErrorMessage = "La orientación de hileras debe estar entre 0° y 360°.")]
    public double OrientacionGrados { get; set; } = 0.0;

    [Range(0.0, 10.0, ErrorMessage = "El margen de cabecera debe estar entre 0.0 m y 10.0 m.")]
    public double MargenCabeceraMetros { get; set; } = 1.5;

    public bool GuardarEnBaseDatos { get; set; } = true;
}

public class HileraViticolaDto
{
    public int Indice { get; set; }
    public double LongitudMetros { get; set; }
    public int CantidadPlantas { get; set; }
    public List<CoordenadaDto> Segmento { get; set; } = new();
}

public class PlantaViticolaDto
{
    public int HileraIndice { get; set; }
    public int PlantaIndice { get; set; }
    public double Lng { get; set; }
    public double Lat { get; set; }
}

public class MetricasViticolaDto
{
    public double AreaMetrosCuadrados { get; set; }
    public double AreaHectareas { get; set; }
    public int TotalFilas { get; set; }
    public int TotalPlantas { get; set; }
    public double DensidadPlantasPorHectarea { get; set; }
    public double LongitudLinealTotalMetros { get; set; }
    public double AlambreEspalderaEstimadoMetros { get; set; }
    public int PostesIntermediosEstimados { get; set; }
    public int PostesCabecerosEstimados { get; set; }
}

public class DisenoParcelaResponseDto
{
    public int? Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string VariedadVid { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public double DistanciaEntreFilas { get; set; }
    public double DistanciaEntrePlantas { get; set; }
    public double OrientacionGrados { get; set; }
    public double MargenCabeceraMetros { get; set; }
    public MetricasViticolaDto Metricas { get; set; } = new();
    public List<HileraViticolaDto> Hileras { get; set; } = new();
    public List<PlantaViticolaDto> Plantas { get; set; } = new();
    public string PoligonoGeoJson { get; set; } = string.Empty;
    public string FilasGeoJson { get; set; } = string.Empty;
    public string PlantasGeoJson { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}

public class ParcelaResumenDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string VariedadVid { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public double AreaMetrosCuadrados { get; set; }
    public double AreaHectareas { get; set; }
    public int TotalFilas { get; set; }
    public int TotalPlantas { get; set; }
    public double DistanciaEntreFilas { get; set; }
    public double DistanciaEntrePlantas { get; set; }
    public double OrientacionGrados { get; set; }
    public DateTime FechaCreacion { get; set; }
}
