using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Qdrant.Client.Grpc;
using Microsoft.Extensions.Configuration;
using ChatBot.Services.Dtos;


public class DeepSeekClient : ILLMClient
{
    private const int MaxTokenLimit = 4000;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly int _maxContextLength;

    public DeepSeekClient(IConfiguration config, HttpClient httpClient)
    {
        _apiKey = config["DeepSeek:ApiKey"] ?? throw new ArgumentNullException("DeepSeek:ApiKey");
        var baseUrl = config["DeepSeek:ApiUrl"] ?? "https://api.deepseek.com/v1";
        _model = config["DeepSeek:Model"] ?? "deepseek-chat";

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        _maxContextLength = (int)(MaxTokenLimit * 0.3);
    }

    public async Task<string> GenerateResponseAsync(string userMessage, IReadOnlyList<ScoredPoint>? entries, CancellationToken cancellationToken = default)
    {
        // 1. Mesajları hazırla (tek method içinde)
        List<object> messages;

        if (entries == null || !entries.Any())
        {
            messages = new List<object>
            {
                new { role = "user", content = userMessage }
            };
        }
        else
        {
            // Context'i işle
            var context = entries
                .OrderByDescending(e =>
                {
                    if (e.Payload.TryGetValue("timestamp", out var timestampValue))
                    {
                        return DateTime.TryParse(timestampValue.StringValue, out var dt)
                            ? dt
                            : DateTime.MinValue;
                    }
                    return DateTime.MinValue;
                })
                .Select(e =>
                {
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

            
            var apiSchemas = LoadApiSchemasAsText();

            var systemMessage = $"Sen bir ödeme sistemleri API uzmanısın. Aşağıdaki API'lerle çalışıyorsun ve sadece bu API'lere göre cevap ver:{apiSchemas} Kullanıcıların API kullanımı sırasında karşılaştıkları hataları çözmelerine yardımcı olacaksın. Hataları analiz edip: 1. Hatanın kaynağını açıkla 2. Çözüm adımlarını madde madde sırala 3. İlgili API dokümantasyonundan örnek request/response göster 4. Türkçe cevap ver. Aynı zamanda cevap verirken geçmiş konuşmaları hatırlayan bir asistansın\n\nBağlam:\n{string.Join("\n---\n", context)}";

            messages = new List<object>
            {
                new { role = "system", content = systemMessage },
                new { role = "user", content = userMessage }
            };
        }

        // 2. API isteğini hazırla ve gönder
        var requestData = new
        {
            model = _model,
            messages,
            max_tokens = MaxTokenLimit,
            temperature = 0.7
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestData),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        // 3. Yanıtı işle (DTO kullanmadan)
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var jsonDoc = JsonDocument.Parse(responseJson);

        return jsonDoc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "Yanıt alınamadı";
    }

    private List<FunctionDefinition> LoadApiFunctions()
    {
        var schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "Services", "apiSchema.json");
        var schemaText = File.ReadAllText(schemaPath);
        var schemaJson = JsonDocument.Parse(schemaText).RootElement;

        var functions = new List<FunctionDefinition>();

        foreach (var endpoint in schemaJson.EnumerateArray())
        {
            functions.Add(new FunctionDefinition
            {
                Name = endpoint.GetProperty("name").GetString(),
                Description = endpoint.GetProperty("description").GetString(),
                Parameters = endpoint.GetProperty("parameters").Clone()
            });
        }

        return functions;
    }
    
    private string LoadApiSchemasAsText()
{
    try
    {
        // 1. Dosya yolunu oluştur ve varlığını kontrol et
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "Services", "apiSchema.json");
        
        if (!File.Exists(schemaPath))
        {
            throw new FileNotFoundException($"API şema dosyası bulunamadı: {schemaPath}");
        }

        // 2. Dosyayı oku ve JSON'ı parse et
        var jsonContent = File.ReadAllText(schemaPath);
        using var schemaJson = JsonDocument.Parse(jsonContent);

        // 3. JSON yapısını kontrol et
        if (schemaJson.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Geçersiz JSON formatı: Dizi bekleniyor");
        }

        // 4. API açıklamalarını oluştur
        var apiDescriptions = new List<string>();
        foreach (var endpoint in schemaJson.RootElement.EnumerateArray())
        {
            try
            {
                apiDescriptions.Add($"""
                    API adı: {endpoint.GetProperty("name").GetString()}
                    Açıklama: {endpoint.GetProperty("description").GetString()}
                    Parametreler: {endpoint.GetProperty("parameters")}
                    """);
            }
            catch (KeyNotFoundException ex)
            {
                // Eksik property durumu
                apiDescriptions.Add($"Hata: {ex.Message} - Geçersiz endpoint yapısı");
            }
        }

        return string.Join("\n\n", apiDescriptions);
    }
    catch (Exception ex)
    {
        // Hata durumunda kullanıcı dostu mesaj
        return $"API şemaları yüklenirken hata oluştu: {ex.Message}";
    }
}

}