using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AgroBusca.Api.Data;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Interfaces;
using AgroBusca.Api.Models;

namespace AgroBusca.Api.Services;

public class FichaTecnicaService : IFichaTecnicaService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FichaTecnicaService> _logger;

    public FichaTecnicaService(AppDbContext context, ILogger<FichaTecnicaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FichaTecnicaDto>> ObtenerTodasAsync(FiltroFichaTecnicaDto? filtro = null, CancellationToken cancellationToken = default)
    {
        var query = _context.FichasTecnicas.AsNoTracking().AsQueryable();

        if (filtro is not null)
        {
            if (!string.IsNullOrWhiteSpace(filtro.Tipo))
            {
                var tipoNormalizado = filtro.Tipo.Trim().ToLower();
                query = query.Where(f => f.Tipo.ToLower() == tipoNormalizado);
            }

            if (!string.IsNullOrWhiteSpace(filtro.CultivoVariedad))
            {
                var cultivoNormalizado = filtro.CultivoVariedad.Trim().ToLower();
                query = query.Where(f => f.CultivoVariedad.ToLower().Contains(cultivoNormalizado));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Zona))
            {
                var zonaNormalizada = filtro.Zona.Trim().ToLower();
                query = query.Where(f => f.Zona.ToLower().Contains(zonaNormalizada));
            }

            if (!string.IsNullOrWhiteSpace(filtro.TerminoBusqueda))
            {
                var term = filtro.TerminoBusqueda.Trim().ToLower();
                query = query.Where(f =>
                    f.Titulo.ToLower().Contains(term) ||
                    f.Descripcion.ToLower().Contains(term) ||
                    f.SintomasOIdentificacion.ToLower().Contains(term) ||
                    f.MedidasManejo.ToLower().Contains(term));
            }
        }

        var fichas = await query
            .OrderBy(f => f.Tipo)
            .ThenBy(f => f.Titulo)
            .Select(f => MapearDto(f))
            .ToListAsync(cancellationToken);

        return fichas;
    }

    public async Task<FichaTecnicaDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var ficha = await _context.FichasTecnicas
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        return ficha is null ? null : MapearDto(ficha);
    }

    public async Task<FichaTecnicaDto> CrearAsync(CrearFichaTecnicaDto dto, CancellationToken cancellationToken = default)
    {
        var entidad = new FichaTecnica
        {
            Titulo = dto.Titulo.Trim(),
            CultivoVariedad = dto.CultivoVariedad.Trim(),
            Tipo = dto.Tipo.Trim(),
            Zona = dto.Zona.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            SintomasOIdentificacion = dto.SintomasOIdentificacion.Trim(),
            MedidasManejo = dto.MedidasManejo.Trim(),
            RequerimientosEdafoclimaticos = dto.RequerimientosEdafoclimaticos.Trim(),
            FuentesReferencia = dto.FuentesReferencia.Trim(),
            FechaActualizacion = DateTime.UtcNow
        };

        _context.FichasTecnicas.Add(entidad);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ficha técnica creada con ID: {Id}, Título: {Titulo}", entidad.Id, entidad.Titulo);
        return MapearDto(entidad);
    }

    public async Task<FichaTecnicaDto?> ActualizarAsync(int id, ActualizarFichaTecnicaDto dto, CancellationToken cancellationToken = default)
    {
        var entidad = await _context.FichasTecnicas.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (entidad is null) return null;

        entidad.Titulo = dto.Titulo.Trim();
        entidad.CultivoVariedad = dto.CultivoVariedad.Trim();
        entidad.Tipo = dto.Tipo.Trim();
        entidad.Zona = dto.Zona.Trim();
        entidad.Descripcion = dto.Descripcion.Trim();
        entidad.SintomasOIdentificacion = dto.SintomasOIdentificacion.Trim();
        entidad.MedidasManejo = dto.MedidasManejo.Trim();
        entidad.RequerimientosEdafoclimaticos = dto.RequerimientosEdafoclimaticos.Trim();
        entidad.FuentesReferencia = dto.FuentesReferencia.Trim();
        entidad.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Ficha técnica ID {Id} actualizada exitosamente.", id);

        return MapearDto(entidad);
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var entidad = await _context.FichasTecnicas.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (entidad is null) return false;

        _context.FichasTecnicas.Remove(entidad);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ficha técnica ID {Id} eliminada.", id);
        return true;
    }

    public async Task InicializarFichasSemillaAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.FichasTecnicas.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Las fichas técnicas ya contienen registros; se omite la inicialización de semillas.");
            return;
        }

        var semillas = new List<FichaTecnica>
        {
            new()
            {
                Titulo = "Clorosis Férrica en Suelos Calcáreos del Valle de Tarija",
                CultivoVariedad = "Vid (Vitis vinifera) - Portainjertos y variedades francas",
                Tipo = "NutricionSuelos",
                Zona = "Valle Central de Tarija (Uriondo, Calamuchita, San Lorenzo)",
                Descripcion = "Trastorno fisiológico recurrente originado por la insolubilización del hierro (Fe) en suelos de alto pH (alcalinos o calcáreos con elevado contenido de carbonato de calcio activo). Impide la síntesis normal de clorofila.",
                SintomasOIdentificacion = "Amarilleamiento internerval característico en hojas jóvenes brotadas en primavera, mientras las nervaduras primarias y secundarias permanecen verdes. En casos severos, desecación marginal y necrosis foliar.",
                MedidasManejo = "1. Uso de portainjertos tolerantes a caliza activa (ej. 110-Richter, Fercal, Ruggeri 140).\n2. Aplicación de quelatos de hierro sintéticos (EDDHA de alta solubilidad) directamente al suelo mediante fertirriego a inicio de brotación.\n3. Corrección orgánica con compost maduro para reducir pH en el bulbo radicular.",
                RequerimientosEdafoclimaticos = "Suelos con carbonato cálcico activo > 8-12%, pH > 7.8. Común en terrazas aluviales del Valle de la Concepción.",
                FuentesReferencia = "CENAVIT / INIAF Tarija - Manual de Fisiopatías de la Vid",
                FechaActualizacion = DateTime.UtcNow
            },
            new()
            {
                Titulo = "Oídio o Cenicilla de la Vid (Uncinula necator / Erysiphe necator)",
                CultivoVariedad = "Vid (Vitis vinifera) - Muy susceptible en Moscatel y Syrah",
                Tipo = "Enfermedad",
                Zona = "Valle de la Concepción, Santa Ana, San Lorenzo",
                Descripcion = "Enfermedad fúngica más limitante y endémica en los viñedos de altura de Tarija. El hongo es ectoparásito y prolifera bajo condiciones de humedad relativa moderada (60-80%) y temperaturas entre 20°C y 28°C sin requerir lluvia libre.",
                SintomasOIdentificacion = "Polvillo blanquecino o ceniciento en ambas caras de la hoja, brotes y especialmente sobre los racimos. Los granos afectados se agrietan, exponiendo las semillas y permitiendo el ataque secundario de Botrytis cinerea.",
                MedidasManejo = "1. Poda verde: deshoje alrededor del racimo para garantizar insolación directa y ventilación.\n2. Tratamientos preventivos con Azufre micronizado / mojable o espolvoreo desde brote de 10-15 cm hasta el envero.\n3. Aplicación de fungicidas triazoles o estrobirulinas en floración y cuajado.",
                RequerimientosEdafoclimaticos = "Clima templado a cálido con radiación solar tamizada por follaje denso. No tolera lluvias copiosas continuas (el agua lava las conidias).",
                FuentesReferencia = "CENAVIT - Guía Sanitaria Vitícola de Tarija",
                FechaActualizacion = DateTime.UtcNow
            },
            new()
            {
                Titulo = "Mildiu de la Vid (Plasmopara viticola)",
                CultivoVariedad = "Vid (Vitis vinifera) - Todas las variedades en brotación estival",
                Tipo = "Enfermedad",
                Zona = "Valle Central de Tarija y Bermejo",
                Descripcion = "Oomiceto causante de tizón y defoliación prematura. Se desencadena tras lluvias estivales copiosas que coinciden con temperaturas superiores a 10-12°C (Regla de los 3 dieces: brotes de 10 cm, 10 mm de lluvia y 10°C).",
                SintomasOIdentificacion = "'Manchas de aceite' translúcidas en el haz de las hojas y posterior eflorescencia algodonosa blanca en el envés. Racimos jóvenes se curvan en 'S' y se necrosan.",
                MedidasManejo = "1. Aplicaciones preventivas con sales de cobre (oxicloruro de cobre, caldo bordelés) tras eventos de precipitación.\n2. Manejo de coberturas y drenaje del viñedo para evitar encharcamientos.",
                RequerimientosEdafoclimaticos = "Humedad saturada (>90%), rocío prolongado y lluvias estivales de diciembre a marzo.",
                FuentesReferencia = "INIAF - Sistema de Alertas Fitovícolas Tarija",
                FechaActualizacion = DateTime.UtcNow
            },
            new()
            {
                Titulo = "Variedad Moscatel de Alejandría (Vid Blanca Insignia)",
                CultivoVariedad = "Moscatel de Alejandría (Muscat of Alexandria)",
                Tipo = "Variedad",
                Zona = "Uriondo (Valle de la Concepción), Calamuchita, Santa Ana",
                Descripcion = "Variedad histórica emblema del Valle Central de Tarija. Materia prima exclusiva con Denominación de Origen para la destilación del Singani boliviano y uva de mesa de alta demanda.",
                SintomasOIdentificacion = "Racimo grande, cónico y suelto. Bayas elípticas de color verde-amarillento dorado al madurar, con sabor intensamente almizclado, floral y aromático (terpenos como linalol y geraniol).",
                MedidasManejo = "Conducción recomendada en espaldera de 3 a 4 alambres o parral tradicional de altura. Poda mixta (Guyot) o en cordón royat con 2 yemas francas para evitar corrimiento del racimo.",
                RequerimientosEdafoclimaticos = "Altitud de 1,700 a 2,000 msnm. Requiere alta heliofanía para maduración aromática terpénica y noches frescas para retener acidez tartárica.",
                FuentesReferencia = "Fundación FAUTAPO / CENAVIT - Caracterización Ampelográfica de la Vid en Tarija",
                FechaActualizacion = DateTime.UtcNow
            },
            new()
            {
                Titulo = "Variedad Tannat (Tinta de Altura)",
                CultivoVariedad = "Tannat",
                Tipo = "Variedad",
                Zona = "Valle Central de Tarija (1,850 - 2,200 msnm)",
                Descripcion = "Cepa tinta originaria del suroeste de Francia que ha encontrado en los Valles Altos de Tarija una adaptación de renombre mundial para vinos tintos de guarda con potente concentración de polifenoles y taninos sedosos gracias a la radiación UV de altura.",
                SintomasOIdentificacion = "Racimo compacto, cilíndrico con alerón. Bayas esféricas, pequeñas, piel gruesa de intenso color azul-negro violáceo.",
                MedidasManejo = "Conducción en espaldera vertical (VSP) con deshoje matutino cuidadoso para evitar golpe de sol en la tarde.",
                RequerimientosEdafoclimaticos = "Suelos franco-arcillosos o pedregosos bien drenados. Alta amplitud térmica (diferencia día-noche de 15°C a 20°C).",
                FuentesReferencia = "ANIV / CENAVIT - Vinos de Altura de Bolivia",
                FechaActualizacion = DateTime.UtcNow
            },
            new()
            {
                Titulo = "Riego por Goteo y Estrés Hídrico Regulado (RDI)",
                CultivoVariedad = "Vid para vinificación (Vitis vinifera)",
                Tipo = "Riego",
                Zona = "Cuenca del Río Guadalquivir / Río Camacho (Tarija)",
                Descripcion = "Técnica de manejo hídrico de precisión que optimiza el consumo de agua en zonas semiáridas y estimula la concentración de azúcares y antocianos en el grano de uva.",
                SintomasOIdentificacion = "Monitoreo con tensiómetros o sensores de humedad de suelo a 30 y 60 cm de profundidad, o cámara de presión Scholander midiendo potencial hídrico foliar.",
                MedidasManejo = "1. Riego pleno desde brotación hasta cuajado.\n2. Estrés hídrico moderado (déficit controlado de 30-40% de la ETc) desde tamaño perdigón hasta envero para frenar crecimiento vegetativo y concentrar la energía en el fruto.\n3. Riego post-cosecha para reposición de reservas en raíces antes de la dormición.",
                RequerimientosEdafoclimaticos = "Evapotranspiración de referencia (ETo) entre 4.5 y 6.0 mm/día en los meses cálidos de Tarija.",
                FuentesReferencia = "CENAVIT - Manual de Riego Tecnificado en Viticultura de Precisión",
                FechaActualizacion = DateTime.UtcNow
            }
        };

        await _context.FichasTecnicas.AddRangeAsync(semillas, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Se inicializaron {Total} fichas técnicas vitícolas semilla.", semillas.Count);
    }

    private static FichaTecnicaDto MapearDto(FichaTecnica f) => new()
    {
        Id = f.Id,
        Titulo = f.Titulo,
        CultivoVariedad = f.CultivoVariedad,
        Tipo = f.Tipo,
        Zona = f.Zona,
        Descripcion = f.Descripcion,
        SintomasOIdentificacion = f.SintomasOIdentificacion,
        MedidasManejo = f.MedidasManejo,
        RequerimientosEdafoclimaticos = f.RequerimientosEdafoclimaticos,
        FuentesReferencia = f.FuentesReferencia,
        FechaActualizacion = f.FechaActualizacion
    };
}
