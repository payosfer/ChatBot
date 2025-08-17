using Volo.Abp.Application.Services;
using ChatBot.Localization;

namespace ChatBot.Services;

/* Inherit your application services from this class. */
public abstract class ChatBotAppService : ApplicationService
{
    protected ChatBotAppService()
    {
        LocalizationResource = typeof(ChatBotResource);
    }
}