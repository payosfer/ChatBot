public class ClientFactory : IClientFactory
{
    private readonly IServiceProvider _services;

    public ClientFactory(IServiceProvider services) 
        => _services = services;

    public ILLMClient CreateClient(LLMProvider provider)
    {
        return provider switch
        {
            LLMProvider.DeepSeek => _services.GetRequiredService<DeepSeekClient>(),
            LLMProvider.OpenAI   => _services.GetRequiredService<OpenAIClient>(),
            _ => throw new ArgumentOutOfRangeException(nameof(provider))
        };
    }
}