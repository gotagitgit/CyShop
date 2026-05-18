namespace Chat.Infrastructure;

public class ChatSettings
{
    public string Provider { get; set; } = "Ollama";

    // Ollama settings
    public string OllamaEndpoint { get; set; } = "http://localhost:11434";
    public string OllamaModelName { get; set; } = "llama3.2:1b";

    // HuggingFace settings
    public string HuggingFaceEndpoint { get; set; } = "https://router.huggingface.co/v1";
    public string HuggingFaceApiKey { get; set; } = string.Empty;
    public string HuggingFaceModelName { get; set; } = "Qwen/Qwen2.5-7B-Instruct:together";

    // Shared settings
    public int TimeoutSeconds { get; set; } = 30;
}
