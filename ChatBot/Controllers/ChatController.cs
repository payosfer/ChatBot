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

[Route("api/chat")]
[ApiController]
public class ChatController : AbpController
{
    private readonly IOpenAiAppService _openAiAppService;

    public ChatController(IOpenAiAppService openAiAppService)
    {
        _openAiAppService = openAiAppService;
    }

    [HttpPost("ask")]
    public async Task<string> Ask([FromBody] ChatRequestDto input)
    {
        // OpenAI'ye mesajları gönderip cevap al
        var response = await _openAiAppService.AskChatGpt(input);
        return response;
    }

// CancellationToken burada ASP.NET Core tarafından otomatik olarak HttpContext.RequestAborted üzerinden enjekte edilir.
}
