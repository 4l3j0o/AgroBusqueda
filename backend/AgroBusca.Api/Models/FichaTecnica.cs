namespace AgroBusca.Api.Models;

public class FichaTecnica
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string CultivoVariedad { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Zona { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string SintomasOIdentificacion { get; set; } = string.Empty;
    public string MedidasManejo { get; set; } = string.Empty;
    public string RequerimientosEdafoclimaticos { get; set; } = string.Empty;
    public string FuentesReferencia { get; set; } = string.Empty;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
