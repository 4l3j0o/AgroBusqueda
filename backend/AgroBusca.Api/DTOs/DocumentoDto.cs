using System.ComponentModel.DataAnnotations;

namespace AgroBusca.Api.DTOs;

public class DocumentoCreateDto
{
    [Required(ErrorMessage = "El título del documento es obligatorio.")]
    [StringLength(250, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 250 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El autor o institución emisora es obligatorio.")]
    [StringLength(150, ErrorMessage = "El autor o institución no puede exceder 150 caracteres.")]
    public string AutorInstitucion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría vitícola es obligatoria.")]
    [StringLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres.")]
    public string Categoria { get; set; } = string.Empty;

    [Url(ErrorMessage = "Debe proporcionar una URL válida para la fuente.")]
    public string? UrlFuente { get; set; }

    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "El contenido textual para fragmentar e indexar es obligatorio.")]
    [MinLength(20, ErrorMessage = "El contenido debe tener al menos 20 caracteres.")]
    public string ContenidoCompleto { get; set; } = string.Empty;

    public string? Subseccion { get; set; }
}

public class DocumentoResponseDto
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string AutorInstitucion { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public string? UrlFuente { get; set; }

    public DateTime FechaPublicacion { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int CantidadFragmentos { get; set; }
}
