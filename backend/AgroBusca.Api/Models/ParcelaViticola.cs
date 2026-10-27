namespace AgroBusca.Api.Models;

public class ParcelaViticola
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string VariedadVid { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string PoligonoGeoJson { get; set; } = string.Empty;
    public double DistanciaEntreFilas { get; set; } = 2.5;
    public double DistanciaEntrePlantas { get; set; } = 1.2;
    public double OrientacionGrados { get; set; } = 0.0;
    public double MargenCabeceraMetros { get; set; } = 1.5;
    public double AreaMetrosCuadrados { get; set; }
    public int TotalFilas { get; set; }
    public int TotalPlantas { get; set; }
    public double MetrosLinealesTotales { get; set; }
    public string FilasGeoJson { get; set; } = string.Empty;
    public string PlantasGeoJson { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
