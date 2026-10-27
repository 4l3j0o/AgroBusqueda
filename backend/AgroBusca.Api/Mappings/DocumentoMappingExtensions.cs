using AgroBusca.Api.DTOs;
using AgroBusca.Api.Models;

namespace AgroBusca.Api.Mappings;

public static class DocumentoMappingExtensions
{
    public static DocumentoResponseDto ToDto(this Documento documento)
    {
        return new DocumentoResponseDto
        {
            Id = documento.Id,
            Titulo = documento.Titulo,
            AutorInstitucion = documento.AutorInstitucion,
            Categoria = documento.Categoria,
            UrlFuente = documento.UrlFuente,
            FechaPublicacion = documento.FechaPublicacion,
            FechaRegistro = documento.FechaRegistro,
            CantidadFragmentos = documento.Fragmentos?.Count ?? 0
        };
    }

    public static FuenteRespuestaDto ToFuenteRespuestaDto(this FragmentoDocumento fragmento, double similitud)
    {
        return new FuenteRespuestaDto
        {
            FragmentoId = fragmento.Id,
            DocumentoId = fragmento.DocumentoId,
            Titulo = fragmento.Documento?.Titulo ?? string.Empty,
            Institucion = fragmento.Documento?.AutorInstitucion ?? string.Empty,
            Categoria = fragmento.Documento?.Categoria ?? string.Empty,
            UrlFuente = fragmento.Documento?.UrlFuente,
            Subseccion = fragmento.Subseccion,
            Fragmento = fragmento.Contenido,
            Similitud = Math.Round(Math.Clamp(similitud, 0.0, 1.0), 4),
            FechaPublicacion = fragmento.Documento?.FechaPublicacion ?? DateTime.MinValue
        };
    }

    public static ResultadoBusquedaDto ToResultadoBusquedaDto(this FragmentoDocumento fragmento, double similitud)
    {
        return new ResultadoBusquedaDto
        {
            FragmentoId = fragmento.Id,
            DocumentoId = fragmento.DocumentoId,
            Titulo = fragmento.Documento?.Titulo ?? string.Empty,
            Institucion = fragmento.Documento?.AutorInstitucion ?? string.Empty,
            Categoria = fragmento.Documento?.Categoria ?? string.Empty,
            UrlFuente = fragmento.Documento?.UrlFuente,
            Subseccion = fragmento.Subseccion,
            Fragmento = fragmento.Contenido,
            Similitud = Math.Round(Math.Clamp(similitud, 0.0, 1.0), 4),
            FechaPublicacion = fragmento.Documento?.FechaPublicacion ?? DateTime.MinValue
        };
    }
}
