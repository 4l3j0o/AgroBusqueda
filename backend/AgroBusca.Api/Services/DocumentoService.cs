using Microsoft.EntityFrameworkCore;
using AgroBusca.Api.Data;
using AgroBusca.Api.DTOs;
using AgroBusca.Api.Interfaces;
using AgroBusca.Api.Mappings;
using AgroBusca.Api.Models;

namespace AgroBusca.Api.Services;

public class DocumentoService : IDocumentoService
{
    private readonly AppDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<DocumentoService> _logger;

    public DocumentoService(
        AppDbContext context,
        IEmbeddingService embeddingService,
        ILogger<DocumentoService> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<DocumentoResponseDto> CrearDocumentoConFragmentacionAsync(DocumentoCreateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registrando y fragmentando nuevo documento: '{Titulo}'", dto.Titulo);

        var documento = new Documento
        {
            Titulo = dto.Titulo.Trim(),
            AutorInstitucion = dto.AutorInstitucion.Trim(),
            Categoria = dto.Categoria.Trim(),
            UrlFuente = dto.UrlFuente?.Trim(),
            FechaPublicacion = dto.FechaPublicacion,
            FechaRegistro = DateTime.UtcNow
        };

        var chunks = FragmentarTexto(dto.ContenidoCompleto);

        for (var i = 0; i < chunks.Count; i++)
        {
            var textoChunk = chunks[i];
            var embedding = await _embeddingService.GenerarEmbeddingAsync(textoChunk, cancellationToken);

            documento.Fragmentos.Add(new FragmentoDocumento
            {
                IndiceChunk = i + 1,
                Contenido = textoChunk,
                Subseccion = dto.Subseccion ?? $"Sección {i + 1}",
                Embedding = embedding
            });
        }

        _context.Documentos.Add(documento);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Documento '{Titulo}' guardado con {Chunks} fragmentos indexados.", documento.Titulo, documento.Fragmentos.Count);

        return documento.ToDto();
    }

    public async Task<IReadOnlyList<DocumentoResponseDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var documentos = await _context.Documentos
            .Include(d => d.Fragmentos)
            .AsNoTracking()
            .OrderByDescending(d => d.FechaRegistro)
            .ToListAsync(cancellationToken);

        return documentos.Select(d => d.ToDto()).ToList();
    }

    public async Task<DocumentoResponseDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var documento = await _context.Documentos
            .Include(d => d.Fragmentos)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        return documento?.ToDto();
    }

    public async Task<int> ObtenerTotalDocumentosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documentos.CountAsync(cancellationToken);
    }

    public async Task<int> ObtenerTotalFragmentosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FragmentosDocumento.CountAsync(cancellationToken);
    }

    public async Task<bool> EliminarDocumentoAsync(int id, CancellationToken cancellationToken = default)
    {
        var documento = await _context.Documentos
            .Include(d => d.Fragmentos)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (documento is null) return false;

        _context.Documentos.Remove(documento);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Documento ID {Id} y sus fragmentos asociados han sido eliminados.", id);
        return true;
    }

    public async Task InicializarDatosSemillaAsync(CancellationToken cancellationToken = default)
    {
        var total = await _context.Documentos.CountAsync(cancellationToken);
        if (total > 0)
        {
            _logger.LogInformation("La base de datos ya contiene {Total} documentos indexados. Omitiendo datos semilla.", total);
            return;
        }

        _logger.LogInformation("Iniciando carga de datos semilla de viticultura para Tarija, Bolivia...");

        var documentosSemilla = ObtenerDocumentosViticulturaTarija();

        foreach (var docDto in documentosSemilla)
        {
            await CrearDocumentoConFragmentacionAsync(docDto, cancellationToken);
        }

        _logger.LogInformation("Carga de datos semilla completada exitosamente.");
    }

    private static List<string> FragmentarTexto(string texto)
    {
        var parrafos = texto.Split(new[] { "\r\n\r\n", "\n\n", "\r\r" }, StringSplitOptions.RemoveEmptyEntries);
        var fragmentos = new List<string>();

        foreach (var p in parrafos)
        {
            var limpio = p.Trim();
            if (limpio.Length >= 40)
            {
                fragmentos.Add(limpio);
            }
        }

        if (fragmentos.Count == 0 && !string.IsNullOrWhiteSpace(texto))
        {
            fragmentos.Add(texto.Trim());
        }

        return fragmentos;
    }

    private static List<DocumentoCreateDto> ObtenerDocumentosViticulturaTarija()
    {
        return new List<DocumentoCreateDto>
        {
            new()
            {
                Titulo = "Guía Técnica: Diagnóstico y Corrección de Clorosis Férrica en Viñedos de Altura de Tarija",
                AutorInstitucion = "CENAVIT (Centro Nacional Vitivinícola de Tarija)",
                Categoria = "Nutrición Vegetal",
                UrlFuente = "https://cenavit.gob.bo/publicaciones/clorosis-ferrica-tarija.pdf",
                FechaPublicacion = new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                Subseccion = "Fisiología y Nutrición",
                ContenidoCompleto =
                    "La clorosis férrica es una de las alteraciones fisiológicas más frecuentes en los viñedos del Valle Central de Tarija (Uriondo, Calamuchita y San Lorenzo). " +
                    "Se manifiesta principalmente por el amarilleamiento o pérdida de color verde en las hojas jóvenes del ápice del sarmiento, mientras que las nervaduras principales se mantienen de un color verde característico.\n\n" +
                    "Esta deficiencia no se debe a la ausencia de hierro en el suelo, sino al alto contenido de caliza activa y carbonato de calcio típico de los suelos aluviales tarijeños, con pH superior a 7.8, lo que bloquea la absorción radical del catión ferroso.\n\n" +
                    "Para el manejo y corrección de la clorosis en vid, se recomienda la aplicación al suelo mediante fertirriego de quelatos sintéticos de alta estabilidad como el Fe-EDDHA (orto-orto) en dosis de 15 a 30 gramos por planta al inicio de la brotación primaveral, complementado con incorporación de materia orgánica compostada para acidificar localmente el bulbo húmedo."
            },
            new()
            {
                Titulo = "Manejo Integrado de Oídio (Uncinula necator / Erysiphe necator) en Uvas de Mesa y Singani",
                AutorInstitucion = "INIAF Tarija (Instituto Nacional de Innovación Agropecuaria y Forestal)",
                Categoria = "Sanidad Vegetal",
                UrlFuente = "https://iniaf.gob.bo/tarija/sanidad/oidio-vitis-vinifera.pdf",
                FechaPublicacion = new DateTime(2023, 11, 20, 0, 0, 0, DateTimeKind.Utc),
                Subseccion = "Fitopatología y Plagas",
                ContenidoCompleto =
                    "El oídio o cenicilla es la principal enfermedad criptogámica que afecta a las variedades viníferas y de uva de mesa en Tarija, especialmente a la Moscatel de Alejandría, Cabernet Sauvignon y Syrah. " +
                    "El síntoma visual diagnóstico es la aparición de un polvillo blanquecino o ceniciento harinoso sobre hojas, zarcillos y racimos tiernos, desprendiendo un olor característico a tierra mojada.\n\n" +
                    "Las infecciones tempranas en el racimo provocan que la piel de la uva se torne rígida e incapaz de expandirse, ocasionando el agrietamiento longitudinal de las bayas durante el crecimiento y facilitando la entrada secundaria de podredumbres.\n\n" +
                    "El control agronómico se basa en aplicaciones preventivas de azufre micronizado mojable o en polvo desde brotes de 10 cm hasta el envero, suspendiendo las aplicaciones con temperaturas superiores a 32 °C para evitar fitotoxicidad en la hoja. Se debe mantener el dosel ventilado mediante desbrotes y deshojes en la zona de racimos."
            },
            new()
            {
                Titulo = "Protocolo de Prevención de Mildiu (Plasmopara viticola) en la Cuenca de Santa Ana y El Portillo",
                AutorInstitucion = "Fundación FAUTAPO - Programa Cadena Uva, Vinos y Singanis",
                Categoria = "Sanidad Vegetal",
                UrlFuente = "https://fautapo.org.bo/recursos/manual-mildiu-valles-tarija.pdf",
                FechaPublicacion = new DateTime(2023, 12, 5, 0, 0, 0, DateTimeKind.Utc),
                Subseccion = "Enfermedades Fúngicas",
                ContenidoCompleto =
                    "El mildiu de la vid es provocado por el oomiceto Plasmopara viticola y aparece con fuerza en Tarija cuando convergen lluvias estivales y temperaturas cálidas. " +
                    "En hojas jóvenes produce la clásica 'mancha de aceite' traslúcida en la cara superior (haz), seguida por una eflorescencia algodonosa blanca en la cara inferior (envés) coincidiendo con la zona lesionada.\n\n" +
                    "El modelo de alerta fitosanitaria en los valles interandinos se basa en la regla 10-10-10: temperatura promedio superior a 10 °C, brotes de la vid mayores a 10 cm y precipitaciones acumuladas de al menos 10 mm en 24 a 48 horas.\n\n" +
                    "El tratamiento preventivo por excelencia consiste en la aplicación de sales de cobre (hidróxido de cobre o caldo bordelés) aplicadas antes de periodos de lluvia pronosticados, asegurando una cobertura foliar homogénea de hojas basales y racimos florales."
            },
            new()
            {
                Titulo = "Estrategias de Riego por Goteo y Riego Deficitario Controlado (RDC) en Viñedos de Altura",
                AutorInstitucion = "CENAVIT Tarija",
                Categoria = "Riego y Fertirriego",
                UrlFuente = "https://cenavit.gob.bo/publicaciones/riego-deficitario-uvas-altura.pdf",
                FechaPublicacion = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                Subseccion = "Manejo Hídrico",
                ContenidoCompleto =
                    "En los valles vitícolas de Tarija, situados entre los 1.700 y 2.100 m.s.n.m., la alta radiación ultravioleta y la tasa de evapotranspiración estival exigen una gestión precisa del agua de riego mediante sistemas presurizados de goteo autocompensante.\n\n" +
                    "La técnica del Riego Deficitario Controlado (RDC) aplicada entre el cuajado y el envero permite inducir un estrés hídrico moderado en la vid, frenando el crecimiento vegetativo excesivo de los pámpanos y concentrando metabolitos secundarios como antocianos y taninos en el hollejo.\n\n" +
                    "Se aconseja monitorear el potencial hídrico foliar a mediodía o la tensión en tensiómetros de suelo situados a 30 y 60 cm de profundidad, manteniendo el bulbo radicular con niveles de humedad entre el 60% y el 70% de la capacidad de campo durante la maduración."
            },
            new()
            {
                Titulo = "Poda de Producción y Conducción en Espaldera para la Variedad Moscatel de Alejandría",
                AutorInstitucion = "ANIV (Asociación Nacional de Industriales Vitivinícolas de Bolivia)",
                Categoria = "Manejo del Viñedo",
                UrlFuente = "https://anivbolivia.org/manuales/poda-moscatel-tarija.pdf",
                FechaPublicacion = new DateTime(2023, 8, 25, 0, 0, 0, DateTimeKind.Utc),
                Subseccion = "Poda y Conducción",
                ContenidoCompleto =
                    "La variedad Moscatel de Alejandría es el pilar de la denominación de origen del Singani boliviano en el Valle de Tarija. Su vigor característico y fertilidad de yemas basales condicionan el sistema de poda invernal que debe realizarse en julio y agosto.\n\n" +
                    "El sistema de conducción más difundido es la espaldera alta con cordón bilateral apitonado (Royat) o Guyot simple, dejando pitones de 2 a 3 yemas francas para evitar sobreproducción que diluya los terpenos aromáticos de la uva.\n\n" +
                    "Durante la primavera se complementa con la poda en verde (desbrote y aclareo de pámpanos dobles), logrando una adecuada intercepción solar y aireación que previene la aparición de Botrytis cinerea (podredumbre gris) en vendimia."
            }
        };
    }
}
