using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Chat.Infrastructure.Factory;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Chat.Infrastructure.Tools;

public class AddToBasketTool(
    IChatHttpClientFactory chatHttpClientFactory,
    ILogger<AddToBasketTool> logger) : IChatTool
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient basketHttpClient = chatHttpClientFactory.Create(ChatHttpClientFactory.BasketApiClientName);

    public AIFunction Create() =>
        AIFunctionFactory.Create(AddToBasketAsync, "AddToBasket",
            "Add a product to the customer's shopping basket. Call this tool when the user says 'add to cart', 'add to basket', 'buy this', or wants to purchase a product. Requires the productId, productName, and unitPrice from a previous search result or conversation context. Do NOT search first if you already have the product details.");

    [Description("Adds a product to the customer's shopping basket by product ID, name, and price")]
    private async Task<string> AddToBasketAsync(
        [Description("The unique product identifier (GUID) from the catalog")] string productId,
        [Description("The display name of the product")] string productName,
        [Description("The unit price of the product")] decimal unitPrice,
        CancellationToken ct = default)
    {
        logger.LogInformation("Tool called: AddToBasket('{ProductId}', '{ProductName}', {UnitPrice})",
            productId, productName, unitPrice);

        if (string.IsNullOrWhiteSpace(productId))
        {
            return "A valid product identifier is required to add an item to the basket.";
        }

        try
        {
            // GET current basket
            var getResponse = await basketHttpClient.GetAsync("/api/basket/", ct);
            if (!getResponse.IsSuccessStatusCode)
            {
                logger.LogWarning("GET /api/basket/ returned {StatusCode}", getResponse.StatusCode);
                return "Sorry, I couldn't retrieve your basket. Please try again.";
            }

            var basket = await getResponse.Content.ReadFromJsonAsync<BasketResponse>(JsonOptions, ct);
            var items = basket?.Items ?? [];

            // Check if product already exists in basket
            var parsedProductId = Guid.Parse(productId);
            var existingItem = items.FirstOrDefault(i => i.ProductId == parsedProductId);
            bool isIncrement = existingItem is not null;

            if (isIncrement)
            {
                existingItem!.Quantity += 1;
            }
            else
            {
                items.Add(new BasketItemModel
                {
                    ProductId = parsedProductId,
                    ProductName = productName,
                    UnitPrice = unitPrice,
                    OldUnitPrice = 0,
                    Quantity = 1,
                    PictureUrl = ""
                });
            }

            // POST updated basket
            var updateDto = new UpdateBasketRequest { Items = items };
            var postResponse = await basketHttpClient.PostAsJsonAsync("/api/basket/", updateDto, JsonOptions, ct);
            if (!postResponse.IsSuccessStatusCode)
            {
                logger.LogWarning("POST /api/basket/ returned {StatusCode}", postResponse.StatusCode);
                return "Sorry, I couldn't add the product to your basket. Please try again.";
            }

            return isIncrement
                ? $"Updated {productName} quantity to {existingItem!.Quantity} in your basket."
                : $"Added {productName} to your basket.";
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error while adding product to basket");
            return "Sorry, I couldn't add the product to your basket. Please try again.";
        }
    }

    // Local DTOs — Chat.Infrastructure does not reference Basket.Application
    private sealed class BasketResponse
    {
        public List<BasketItemModel> Items { get; set; } = [];
    }

    private sealed class UpdateBasketRequest
    {
        public List<BasketItemModel> Items { get; set; } = [];
    }

    private sealed class BasketItemModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public decimal OldUnitPrice { get; set; }
        public int Quantity { get; set; }
        public string PictureUrl { get; set; } = "";
    }
}
