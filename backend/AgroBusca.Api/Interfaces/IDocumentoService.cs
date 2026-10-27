using AgroBusca.Api.DTOs;

namespace AgroBusca.Api.Interfaces;

public interface IDocumentoService
{
    Task<DocumentoResponseDto> CrearDocumentoConFragmentacionAsync(DocumentoCreateDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentoResponseDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);

    Task<DocumentoResponseDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> ObtenerTotalDocumentosAsync(CancellationToken cancellationToken = default);

    Task<int> ObtenerTotalFragmentosAsync(CancellationToken cancellationToken = default);

    Task<bool> EliminarDocumentoAsync(int id, CancellationToken cancellationToken = default);

    Task InicializarDatosSemillaAsync(CancellationToken cancellationToken = default);
}
