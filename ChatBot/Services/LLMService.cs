using ChatBot.Services.Dtos;
using ChatBot.Services;
using Volo.Abp.Users;
using ChromaDB.Client.Models;

using Qdrant.Client;
using Qdrant.Client.Grpc;
using Volo.Abp.Modularity;
using System.Net.Http;
using System.Diagnostics;

public class LLMService : ILLMService
{
    private readonly IClientFactory _clientFactory;
    private readonly ICurrentUser _currentUser;
    private readonly IEmbedding _embedding;
    private readonly QdrantClient _qdrantClient;
    
    private readonly Stopwatch _stopwatch;  // Stopwatch için private field


    public LLMService(
        IClientFactory clientFactory,
        ICurrentUser currentUser,
        IEmbedding embedding)
    {
        _clientFactory = clientFactory;
        _currentUser = currentUser;
        _embedding = embedding;
        _qdrantClient = new QdrantClient("localhost", 6334, apiKey: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2Nlc3MiOiJtIn0.bsszKREkccZRdGmdrYICCL0X4Xh90Hd3C_-I42y8S5E"); // Qdrant'ın varsayılan portu
        Stopwatch _stopwatch = new Stopwatch();        // Stopwatch'ı başlatmak için
    }

    public async Task<string> GenerateCompletionAsync(ChatRequestDto input,LLMProvider provider, CancellationToken cancellationToken = default)
    {
        // 1. Client seçimi (Factory Pattern)
        var client = _clientFactory.CreateClient(provider);

        var userId = _currentUser.Id?.ToString() ?? throw new Exception("Kullanıcı giriş yapmamış");

        const string CollectionPrefix = "collection_";
        var collectionName = CollectionPrefix + userId;


        if (!await _qdrantClient.CollectionExistsAsync(collectionName, cancellationToken))
        {
            await _qdrantClient.CreateCollectionAsync(
                collectionName,
                new VectorParams { Size = 1536, Distance = Distance.Cosine }, // OpenAI embedding boyutu genelde 384/1536
                cancellationToken: cancellationToken
            );
            Console.WriteLine($"🆕 Koleksiyon oluşturuldu: {collectionName}");
        }

        // 1. Embedding al
        var vector = await _embedding.GetEmbeddingAsync(input.Message, cancellationToken);

        Console.WriteLine($"Embedding boyutu: {vector.Length}");

        // 2. Qrant üzerinden context al
        var entry = await _qdrantClient.SearchAsync(
            collectionName,
            vector,
            limit: 5,
            cancellationToken: cancellationToken
        );

        Console.WriteLine($"🔍 Sorgu sonucu: {entry[0].Payload["message"]}");


    _stopwatch.Start();
        // 3. Prompt hazırla ve modeli çalıştır
        var result = await client.GenerateResponseAsync(input.Message, entry, cancellationToken);
    _stopwatch.Stop();
    Console.WriteLine($"🕒 Model çalıştırma süresi: {_stopwatch.Elapsed}");


        // 4. Sonucu döndür
        Console.WriteLine($"🤖 OpenAI cevabı: {result}");

        // 5. User messajını Chroma'ya ekle
        var points = new PointStruct
        {
            Id = (PointId)Guid.NewGuid(),
            Vectors = vector,
            Payload = // metadata
            {
                ["message"] = input.Message,
                ["role"] = input.Role,
                ["timestamp"] = DateTime.UtcNow.ToString("o"),
                ["userId"] = userId
            }
        };
        var updateResult = await _qdrantClient.UpsertAsync(collectionName,  new[] { points }, cancellationToken: cancellationToken);

        Console.WriteLine(updateResult);

        // 6. Assistant cevabını da Chroma'ya ekle
        var assistantPoints = new PointStruct
        {
            Id = (PointId)Guid.NewGuid(),
            Vectors = await _embedding.GetEmbeddingAsync(result, cancellationToken),
            Payload =
            {
                ["message"] = result,
                ["role"] = "assistant",
                ["timestamp"] = DateTime.UtcNow.ToString("o")
            }
        };
        await _qdrantClient.UpsertAsync(collectionName, new[] { assistantPoints }, cancellationToken: cancellationToken);
        Console.WriteLine("✅ Chat DB'ye kaydedildi.");
        return result;
    }
}
