using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;

using ChatBot.Services;

public class OpenAIEmbedding : IEmbedding
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model = "text-embedding-ada-002"; // model ismi özelleştirilebilir

    public OpenAIEmbedding(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException(nameof(configuration), "API Key cannot be null");
    }

    public async Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            input = input,
            model = _model
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

        var embeddingJson = document.RootElement.GetProperty("data")[0].GetProperty("embedding");
        return embeddingJson.EnumerateArray().Select(x => x.GetSingle()).ToArray();

    }




}
