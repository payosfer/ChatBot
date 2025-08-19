using System.Text.Json;

namespace ChatBot.Services.Dtos;

public class FunctionDefinition
{
    public  string? Name { get; set; }
    public  string? Description { get; set; }
    public JsonElement Parameters { get; set; }
}
