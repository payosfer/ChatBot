using OpenAI.Chat;
using OpenAI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Services.Dtos;
using Qdrant.Client.Grpc;
using System.Text.Json;

public class OpenAIClient : ILLMClient
{
    private const int MaxTokenLimit = 4000;
    private readonly ChatClient _client;
    private readonly int _maxContextLength;
    //private readonly List<ChatTool> _tools;

    public OpenAIClient(IConfiguration config)
    {
        var _apiKey = config["OpenAI:ApiKey"] ?? throw new ArgumentNullException(nameof(config), "API Key cannot be null");
        _client = new ChatClient(model: "gpt-3.5-turbo", apiKey: _apiKey); // Örnek model ismi, ihtiyaca göre değiştirilebilir

        _maxContextLength = (int)(MaxTokenLimit * 0.3);

       // _tools = LoadApiFunctions("apiSchema.json");

    }

    public async Task<string> GenerateResponseAsync(string userMessage, IReadOnlyList<ScoredPoint>? entries, CancellationToken cancellationToken = default)
    {
        if (entries == null || !entries.Any()) // Eğer Vector DB'den gelen entry yoksa, direkt kullanıcı mesajını kullan
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

    private List<ChatTool> LoadApiFunctions(string apiSchema)
    {
        var schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "Services", apiSchema);
        var schemaText = File.ReadAllText(schemaPath);
        using var schemaJson = JsonDocument.Parse(schemaText);

        var tools = new List<ChatTool>();

        foreach (var endpoint in schemaJson.RootElement.EnumerateArray())
        {
            // OpenAI SDK'nın önerdiği şekilde CreateFunctionTool kullanıyoruz
            tools.Add(ChatTool.CreateFunctionTool(
                functionName: endpoint.GetProperty("name").GetString() ??
                    throw new InvalidOperationException("Function name is required"),
                functionDescription: endpoint.GetProperty("description").GetString(),
                functionParameters: BinaryData.FromObjectAsJson(
                    JsonSerializer.SerializeToElement(endpoint.GetProperty("parameters")))
            ));

        }


        return tools;
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
        return $"Sen bir ödeme sistemleri API uzmanısın.Aşağıdaki API'lerle çalışıyorsun ve sadece bu API'lere göre cevap ver:{LoadApiSchemasAsText()} Kullanıcıların API kullanımı sırasında karşılaştıkları hataları çözmelerine yardımcı olacaksın. Hataları analiz edip: 1. Hatanın kaynağını açıkla 2. Çözüm adımlarını madde madde sırala 3. İlgili API dokümantasyonundan örnek request/response göster 4. Türkçe cevap ver. Aynı zamanda cevap verirken geçmiş konuşmaları hatırlayan bir asistansın\n\nBağlam:\n{string.Join("\n---\n", context)}";
    }    
        private string LoadApiSchemasAsText()
    {
        var schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "Services", "apiSchema.json");
        var schemaJson = JsonDocument.Parse(File.ReadAllText(schemaPath));
        
        var apiDescriptions = new List<string>();
        foreach (var endpoint in schemaJson.RootElement.EnumerateArray())
        {
            apiDescriptions.Add($"""
                API adı: {endpoint.GetProperty("name").GetString()}
                Açıklama: {endpoint.GetProperty("description").GetString()}
                Parametreler: {endpoint.GetProperty("parameters")}
                """);
        }

        return string.Join("\n\n", apiDescriptions);
    }

}