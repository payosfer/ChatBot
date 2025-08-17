public interface IClientFactory
{
    ILLMClient CreateClient(LLMProvider provider);
}

