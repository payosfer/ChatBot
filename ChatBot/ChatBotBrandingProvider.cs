using Microsoft.Extensions.Localization;
using ChatBot.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace ChatBot;

[Dependency(ReplaceServices = true)]
public class ChatBotBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<ChatBotResource> _localizer;

    public ChatBotBrandingProvider(IStringLocalizer<ChatBotResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}