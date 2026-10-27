using AgroBusca.Api.DTOs;

namespace AgroBusca.Api.Interfaces;

public interface IRagService
{
    Task<(string? Sintesis, string? Modelo, long TiempoMs)> GenerarSintesisRagAsync(
        string pregunta,
        IReadOnlyList<FuenteRespuestaDto> fragmentos,
        CancellationToken cancellationToken = default);
}
