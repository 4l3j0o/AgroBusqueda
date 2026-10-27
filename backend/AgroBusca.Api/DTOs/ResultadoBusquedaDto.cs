using System.Text.Json.Serialization;

namespace AgroBusca.Api.DTOs;

public class FuenteRespuestaDto
{
    public int FragmentoId { get; set; }

    public int DocumentoId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    [JsonPropertyName("tituloDocumento")]
    public string TituloDocumento
    {
        get => Titulo;
        set => Titulo = value;
    }

    public string Institucion { get; set; } = string.Empty;

    [JsonPropertyName("autorInstitucion")]
    public string AutorInstitucion
    {
        get => Institucion;
        set => Institucion = value;
    }

    public string Categoria { get; set; } = string.Empty;

    public string? UrlFuente { get; set; }

    public string? Subseccion { get; set; }

    public string Fragmento { get; set; } = string.Empty;

    [JsonPropertyName("contenido")]
    public string Contenido
    {
        get => Fragmento;
        set => Fragmento = value;
    }

    public double Similitud { get; set; }

    public DateTime FechaPublicacion { get; set; }
}

public class ResultadoBusquedaDto : FuenteRespuestaDto
{
}

public class BusquedaResponseDto
{
    public string Consulta { get; set; } = string.Empty;

    public int TotalRecuperados { get; set; }

    public long TiempoRespuestaMs { get; set; }

    public string? Respuesta { get; set; }

    [JsonPropertyName("sintesisIa")]
    public string? SintesisIa
    {
        get => Respuesta;
        set => Respuesta = value;
    }

    public string? ModeloIa { get; set; }

    public long? TiempoIaMs { get; set; }

    public List<FuenteRespuestaDto> Fuentes { get; set; } = new();

    [JsonPropertyName("resultados")]
    public List<FuenteRespuestaDto> Resultados
    {
        get => Fuentes;
        set => Fuentes = value;
    }

    public string AvisoAgronomico { get; set; } = 
        "Aviso: La información presentada proviene de fuentes técnicas y científicas recuperadas por similitud semántica. " +
        "No constituye un diagnóstico agrícola definitivo ni una prescripción agronómica. Para tratamientos fitosanitarios " +
        "o nutricionales en campo, consulte a un profesional agrónomo calificado y considere las condiciones específicas del valle.";
}
