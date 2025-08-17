using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using ChatBot.Services;
using ChatBot.Services.Dtos;
using System.Threading.Tasks;

namespace ChatBot.Controllers;

/*  
    Dependency Injection uygulandi
    OpenAiAppService kullanilsaydi onun nesnesini olusturmamiz lazimdi
    burada nesne olusturmadan islem yapılabiliriz
*/

[Route("api/chatcontext")]
[ApiController]
public class ChatContextController : AbpController
{
    private readonly ILLMService _llmService;

    public ChatContextController(ILLMService llmService)
    {
        _llmService = llmService;
    }

    [HttpPost("ask")]
    public async Task<string> Ask(
        [FromBody] ChatRequestDto input,
        [FromQuery] LLMProvider provider = LLMProvider.DeepSeek,
        CancellationToken cancellationToken = default)
    {
        return await _llmService.GenerateCompletionAsync(input, provider, cancellationToken);
    }

// CancellationToken burada ASP.NET Core tarafından otomatik olarak HttpContext.RequestAborted üzerinden enjekte edilir.
}
