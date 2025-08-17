using ChatBot.Services.Dtos;

namespace ChatBot.Services;
public interface ILLMService
{
    Task<string> GenerateCompletionAsync(ChatRequestDto input,LLMProvider provider, CancellationToken cancellationToken = default);
}

// cancellationToken, kullanıcı requesti iptal etmesi veya sekmeyi kapatması durumunda async islemi durdurur