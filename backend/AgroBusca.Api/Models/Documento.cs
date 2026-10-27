using System.Text.Json.Serialization;

namespace AgroBusca.Api.Models;

public class Documento
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string AutorInstitucion { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public string? UrlFuente { get; set; }

    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<FragmentoDocumento> Fragmentos { get; set; } = new List<FragmentoDocumento>();
}
