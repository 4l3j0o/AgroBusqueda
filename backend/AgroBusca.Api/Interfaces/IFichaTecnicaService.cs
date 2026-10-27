using AgroBusca.Api.DTOs;

namespace AgroBusca.Api.Interfaces;

public interface IFichaTecnicaService
{
    Task<IReadOnlyList<FichaTecnicaDto>> ObtenerTodasAsync(FiltroFichaTecnicaDto? filtro = null, CancellationToken cancellationToken = default);
    Task<FichaTecnicaDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<FichaTecnicaDto> CrearAsync(CrearFichaTecnicaDto dto, CancellationToken cancellationToken = default);
    Task<FichaTecnicaDto?> ActualizarAsync(int id, ActualizarFichaTecnicaDto dto, CancellationToken cancellationToken = default);
    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default);
    Task InicializarFichasSemillaAsync(CancellationToken cancellationToken = default);
}
