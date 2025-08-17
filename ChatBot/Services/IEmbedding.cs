
public interface IEmbedding
{
    Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default);
}