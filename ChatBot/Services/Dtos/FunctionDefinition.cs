using System.Text.Json;

namespace ChatBot.Services.Dtos;

public class FunctionDefinition
{
    public  string? name { get; set; }
    public  string? description { get; set; }
    public JsonElement parameters { get; set; }
}
