using Qdrant.Client.Grpc;  // ScoredPoint sınıfı için gerekli
public interface ILLMClient
{
    Task<string> GenerateResponseAsync(string userMessage, IReadOnlyList<ScoredPoint>? entries, CancellationToken cancellationToken = default);
}