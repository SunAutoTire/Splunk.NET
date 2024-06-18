using System.Text.Json.Serialization;

namespace SunAuto.Logging.Api;

public class MessageAndContent
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    [JsonPropertyName("content")]
    public object? Content { get; set; }
}
