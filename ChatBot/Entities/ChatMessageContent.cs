using System;
using Volo.Abp.Domain.Entities;
using System.Text.Json.Serialization;

namespace ChatBot.Entities
{
    public class ChatMessageContent : Entity<Guid>
    {

        public Guid? SessionId { get; set; } // Kullanici oturum ID si
        public string Role { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string? Response { get; set; }        // OpenAi cevabindan sonra atanacak
        public DateTime Timestamp { get; private set; }

        private ChatMessageContent() { }            // EF Core için

        public ChatMessageContent(Guid? sessionId, string role, string message, string? response = null)
        {
            Id = Guid.NewGuid();                    // Otomatik Primary Key ataması
            SessionId = sessionId;
            Role = role;
            Message = message;
            Response = response;
            Timestamp = DateTime.UtcNow;
        }   

        public void SetResponse(string response)
        {
            Response = response;
        }
    }
}
