using System.Collections.Generic;
using System.Threading.Tasks;
using ChatBot.Entities; // OpenAiThreadCacheDetails burada tanimlandi
//using ChatBot.Migrations;
using ChatBot.Services.Dtos;

namespace ChatBot.Services;

public interface IOpenAiAppService
{
    // OpenAI yapay zeka modeline request islemini gerceklestirtecek method
    Task<string> AskChatGpt(ChatRequestDto messagesThread);
}
