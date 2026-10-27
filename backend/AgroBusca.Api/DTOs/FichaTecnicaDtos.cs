using System.ComponentModel.DataAnnotations;

namespace AgroBusca.Api.DTOs;

public class FichaTecnicaDto
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
    public DateTime FechaActualizacion { get; set; }
}

public class CrearFichaTecnicaDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(250, ErrorMessage = "El título no puede exceder 250 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cultivo o variedad es obligatorio.")]
    [StringLength(150)]
    public string CultivoVariedad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de ficha es obligatorio (ej. Variedad, Enfermedad, Plaga, Manejo).")]
    [StringLength(80)]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La zona geográfica vitícola es obligatoria.")]
    [StringLength(150)]
    public string Zona { get; set; } = "Valle Central de Tarija";

    [Required(ErrorMessage = "La descripción técnica es obligatoria.")]
    public string Descripcion { get; set; } = string.Empty;

    public string SintomasOIdentificacion { get; set; } = string.Empty;
    public string MedidasManejo { get; set; } = string.Empty;
    public string RequerimientosEdafoclimaticos { get; set; } = string.Empty;

    [StringLength(300)]
    public string FuentesReferencia { get; set; } = "CENAVIT / INIAF Tarija";
}

public class ActualizarFichaTecnicaDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(250)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cultivo o variedad es obligatorio.")]
    [StringLength(150)]
    public string CultivoVariedad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(80)]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La zona es obligatoria.")]
    [StringLength(150)]
    public string Zona { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    public string Descripcion { get; set; } = string.Empty;

    public string SintomasOIdentificacion { get; set; } = string.Empty;
    public string MedidasManejo { get; set; } = string.Empty;
    public string RequerimientosEdafoclimaticos { get; set; } = string.Empty;

    [StringLength(300)]
    public string FuentesReferencia { get; set; } = string.Empty;
}

public class FiltroFichaTecnicaDto
{
    public string? TerminoBusqueda { get; set; }
    public string? Tipo { get; set; }
    public string? CultivoVariedad { get; set; }
    public string? Zona { get; set; }
}
