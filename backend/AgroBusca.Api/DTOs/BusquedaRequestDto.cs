using System.ComponentModel.DataAnnotations;

namespace AgroBusca.Api.DTOs;

public class BusquedaRequestDto
{
    [Required(ErrorMessage = "La consulta o pregunta es obligatoria.")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "La consulta debe tener entre 3 y 500 caracteres.")]
    public string Pregunta { get; set; } = string.Empty;

    public string? Categoria { get; set; }

    [Range(1, 20, ErrorMessage = "El límite de resultados debe estar entre 1 y 20.")]
    public int TopK { get; set; } = 5;

    [Range(0.0, 1.0, ErrorMessage = "El umbral de similitud debe estar entre 0.0 y 1.0.")]
    public double UmbralMinimo { get; set; } = 0.20;

    public bool GenerarRespuestaIa { get; set; } = true;
}
