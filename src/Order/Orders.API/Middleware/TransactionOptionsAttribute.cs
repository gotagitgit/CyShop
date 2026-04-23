namespace Orders.API.Middleware;

/// <summary>
/// Controls how the <see cref="TransactionMiddleware"/> handles a specific endpoint.
/// Apply to endpoint methods or controller classes to customize transaction behavior.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class TransactionOptionsAttribute : Attribute
{
    /// <summary>
    /// When <c>true</c>, the endpoint is excluded from transaction wrapping entirely.
    /// The pipeline delegate is invoked directly with no transaction or buffering.
    /// Default is <c>false</c>.
    /// </summary>
    public bool Skip { get; set; }

    /// <summary>
    /// When <c>false</c>, the transaction is still applied but response buffering is disabled.
    /// This means retries will not produce a clean response — use for large-payload endpoints
    /// where memory pressure from buffering is a concern.
    /// Default is <c>true</c>.
    /// </summary>
    public bool BufferResponse { get; set; } = true;
}
