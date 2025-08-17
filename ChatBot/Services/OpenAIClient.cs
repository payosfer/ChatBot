using OpenAI.Chat;
using OpenAI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChromaDB.Client.Models;

using Qdrant.Client.Grpc;

public class OpenAIClient : ILLMClient
{
    private const int MaxTokenLimit = 4000;
    private readonly ChatClient _client;
    private readonly int _maxContextLength;

    public OpenAIClient(IConfiguration config)
    {
        var _apiKey = config["OpenAI:ApiKey"] ?? throw new ArgumentNullException(nameof(config), "API Key cannot be null");
        _client = new ChatClient(model: "gpt-3.5-turbo", apiKey: _apiKey); // Örnek model ismi, ihtiyaca göre değiştirilebilir

        _maxContextLength = (int)(MaxTokenLimit * 0.3);
    }

    public async Task<string> GenerateResponseAsync(string userMessage, IReadOnlyList<ScoredPoint>? entries, CancellationToken cancellationToken = default)
    {
        if (entries == null || !entries.Any()) // Eğer Chroma'dan gelen entry yoksa, direkt kullanıcı mesajını kullan
        {
            var messages1 = new List<ChatMessage>
            {
                new UserChatMessage(userMessage)
            };

            ChatCompletion completion1 = await _client.CompleteChatAsync(messages1, cancellationToken: cancellationToken);
            return completion1.Content[0].Text;
        }

        //Qdrant'tan gelen ScoredPoint'leri işle
        var context = ProcessEntries(entries);

        var systemMessage = CreateSystemMessage(context);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemMessage),
            new UserChatMessage(userMessage)
        };

        ChatCompletion completion = await _client.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        return completion.Content[0].Text;
    }

    private List<string> ProcessEntries(IReadOnlyList<ScoredPoint> entries)
    {
        return entries
            .OrderByDescending(e =>
            {
                if (e.Payload.TryGetValue("timestamp", out var timestampValue))
                {
                    return DateTime.TryParse(timestampValue.StringValue, out var dt)
                        ? dt
                        : DateTime.MinValue;
                }
                return DateTime.MinValue;
            }
            )
            .Select(e =>
            {
                // Payload'dan metadata çekme (Qdrant'ta Document yerine Payload kullanılır)
                var role = e.Payload.TryGetValue("role", out var roleValue)
                    ? roleValue.StringValue
                    : string.Empty;

                var message = e.Payload.TryGetValue("message", out var messageValue)
                    ? messageValue.StringValue
                    : string.Empty;

                return $"{role}: {message}";
            })
            .Where(doc => !string.IsNullOrWhiteSpace(doc))
            .Take(_maxContextLength)
            .ToList();
    }
    private string CreateSystemMessage(List<string> context)
    {
        return $"Sen bir ödeme sistemleri API uzmanısın. Kullanıcıların API kullanımı sırasında karşılaştıkları hataları çözmelerine yardımcı olacaksın. Hataları analiz edip: 1. Hatanın kaynağını açıkla 2. Çözüm adımlarını madde madde sırala 3. İlgili API dokümantasyonundan örnek request/response göster 4. Türkçe cevap ver. Aynı zamanda cevap verirken geçmiş konuşmaları hatırlayan bir asistansın\n\nBağlam:\n{string.Join("\n---\n", context)}";
    }
}