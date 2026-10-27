namespace AgroBusca.Api.Models;

public class FragmentoDocumento
{
    public int Id { get; set; }

    public int DocumentoId { get; set; }

    public Documento Documento { get; set; } = null!;

    public int IndiceChunk { get; set; }

    public string Contenido { get; set; } = string.Empty;

    public string? Subseccion { get; set; }

    public float[] Embedding { get; set; } = Array.Empty<float>();
}
