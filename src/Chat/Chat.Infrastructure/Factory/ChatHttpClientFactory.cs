namespace Chat.Infrastructure.Factory;

internal sealed class ChatHttpClientFactory(IHttpClientFactory httpClientFactory) : IChatHttpClientFactory
{
    public const string BasketApiClientName = "BasketApi";

    public HttpClient Create(string httpClientName) => httpClientFactory.CreateClient(httpClientName);
}
