using ChatBot.Entities;

namespace ChatBot.Services.Dtos;

public class ChatRequestDto
{
    // Messages null olamaz o yuzden reaquired anahtar kelimesi kullanildi
    public required string Role { get; set; } // "user", "assistant", "system"
    public required string Message { get; set; }
}
