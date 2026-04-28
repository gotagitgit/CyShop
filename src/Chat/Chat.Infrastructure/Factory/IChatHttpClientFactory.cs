namespace Chat.Infrastructure.Factory;

public interface IChatHttpClientFactory
{
    HttpClient Create(string httpClientName);
}
