using AgroBusca.Api.DTOs;

namespace AgroBusca.Api.Interfaces;

public interface IBusquedaService
{
    Task<BusquedaResponseDto> BuscarSemanticaAsync(BusquedaRequestDto request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ObtenerCategoriasAsync(CancellationToken cancellationToken = default);
}
