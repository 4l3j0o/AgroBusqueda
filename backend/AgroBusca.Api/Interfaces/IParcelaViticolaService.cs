using AgroBusca.Api.DTOs;

namespace AgroBusca.Api.Interfaces;

public interface IParcelaViticolaService
{
    Task<DisenoParcelaResponseDto> CalcularYDisenarParcelaAsync(DisenoParcelaRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParcelaResumenDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
    Task<DisenoParcelaResponseDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default);
}
