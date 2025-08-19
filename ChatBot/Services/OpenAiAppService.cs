using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using ChatBot.Entities; // OpenAiThreadCacheDetails burada olmalı
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Application.Services;  // ApplicationService icin
using JetBrains.Annotations;
using ChatBot.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Volo.Abp;
using ChatBot.Services.Dtos;
using Volo.Abp.Users;

namespace ChatBot.Services
{
    [RemoteService(true)]
    public class OpenAiAppService : ApplicationService, IOpenAiAppService, ITransientDependency
    {
        private readonly string _apiUrl;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly ChatBotDbContext _dbContext;
        //        protected readonly ICurrentUser _currentUser;

        public OpenAiAppService(IConfiguration config, HttpClient httpClient, ChatBotDbContext dbContext)
        {
            _apiUrl = config["OpenAI:ApiUrl"] ?? throw new ArgumentNullException(nameof(config), "API URL cannot be null");
            _apiKey = config["OpenAI:ApiKey"] ?? throw new ArgumentNullException(nameof(config), "API Key cannot be null");

            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            _dbContext = dbContext;

            //           _currentUser = CurrentUser;
        }


        /*    public async Task<string> AskChatGpt([FromBody] ChatMessageContent messagesThread)
            {
                Console.WriteLine($"📤 Kullanıcı mesajı alındı: {messagesThread.Message}");

                // 1. API isteği oluşturuluyor
                var requestContent = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                             new { role = "user", content = messagesThread.Message }
                        },
                    temperature = 1,
                    max_tokens = 2048
                };


                // 2. API isteği gönderiliyor
                var response = await _httpClient.PostAsJsonAsync(_apiUrl, requestContent);
                response.EnsureSuccessStatusCode();

                var rawJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine("🔍 OpenAI RAW RESPONSE:\n" + rawJson);

                // 3. JSON formatlama ve cevap alma
                var result = await response.Content.ReadFromJsonAsync<JsonDocument>();
                var reply = result?.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "No response";

                Console.WriteLine($"🤖 OpenAI cevabı: {reply}");

                // 4. response a atma
                messagesThread.Response = reply;

                // 5. Chati DB ye kaydet
                _dbContext.ChatMessageContent.Add(messagesThread);
                await _dbContext.SaveChangesAsync();
                Console.WriteLine("✅ Chat DB'ye kaydedildi.");

                return reply;   
            }
        */

        [HttpPost("ask-chat-gpt")]
        public async Task<string> AskChatGpt([FromBody] ChatRequestDto input)
        {
            Console.WriteLine($"📤 Kullanıcı mesajı alındı: {input.Message}");

           

            var schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "Services", "apiSchema.json");
            var schemaText = await File.ReadAllTextAsync(schemaPath);
            var schemaJson = JsonDocument.Parse(schemaText).RootElement;       
 
            var functionList = new List<FunctionDefinition>(); // Her endpoint eklenecek liste

            foreach (var item in schemaJson.EnumerateArray()) // EnumerateArray schemaJson ın her elemanını sırayla item olarak alır
            {
                var func = new FunctionDefinition
                {
                    Name = item.GetProperty("name").GetString(),
                    Description = item.GetProperty("description").GetString(),
                    Parameters = item.GetProperty("parameters").Clone() 
                };

                functionList.Add(func);
            }



            // 1. API isteği oluşturuluyor
            var requestContent = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = input.Message }
                },
                functions = functionList,
                function_call = "auto",
                temperature = 0.7, // look at this
                max_tokens = 2048
            };

            // 2. API isteği gönderiliyor
            var response = await _httpClient.PostAsJsonAsync(_apiUrl, requestContent);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine("❌ OpenAI API Hatası:\n" + err);
                throw new Exception("OpenAI API call failed: " + err);
            }


            var rawJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine("🔍 OpenAI RAW RESPONSE:\n" + rawJson);


            // 3. JSON formatlama ve cevap alma
            var result = await response.Content.ReadFromJsonAsync<JsonDocument>();
            var reply = result?.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "No response";

            Console.WriteLine($"🤖 OpenAI cevabı: {reply}");

            var userId = CurrentUser.Id;
            // 4. response a atma
            var chatEntity = new ChatMessageContent(
                sessionId: userId,
                role: "user",
                message: input.Message,
                response: reply
            );
            Console.WriteLine("currentUserID" + chatEntity.SessionId);

            // 5. Chati DB ye kaydet
            _dbContext.ChatMessageContent.Add(chatEntity);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine("✅ Chat DB'ye kaydedildi.");

            return reply;
        }
    }
}