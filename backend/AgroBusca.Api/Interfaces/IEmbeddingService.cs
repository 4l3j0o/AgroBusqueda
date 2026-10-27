namespace AgroBusca.Api.Interfaces;

public interface IEmbeddingService
{
    int Dimension { get; }

    string Proveedor { get; }

    Task<float[]> GenerarEmbeddingAsync(string texto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<float[]>> GenerarEmbeddingsBatchAsync(IReadOnlyList<string> textos, CancellationToken cancellationToken = default);
}
