namespace ChromaDB.Client.Models;
public interface IVectorDb
{
    Task CreateCollectionAsync(string collectionName, Dictionary<string, object>? metadata = null,string? tenant = null, string? database = null);
    Task AddEmbeddingAsync(string collectionName, string[] ids, float[][] embeddings, string[] documents, Dictionary<string, object>[]? metadatas = null);
    Task<List<ChromaCollectionQueryEntry>> QueryAsync(string collectionName, float[] embedding, int topN, ChromaQueryInclude? include = null);
}
