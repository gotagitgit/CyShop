namespace Chat.Infrastructure;

public class ChatSettings
{
    public string OllamaEndpoint { get; set; } = "http://localhost:11434";
    public string ModelName { get; set; } = "llama3.2:1b";
    public int TimeoutSeconds { get; set; } = 30;
}
