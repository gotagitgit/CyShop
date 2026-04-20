using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Auth.Infrastructure.Dtos;

public class CreateClientDto
{
    public string ClientId { get; init; }

    public bool Enabled { get; init; } = true;

    public bool PublicClient { get; init; } = true;

    public bool DirectAccessGrantsEnabled { get; init; } = true;

    public bool StandardFlowEnabled { get; init; } = true;

    public string[] RedirectUris { get; init; }

    public string[] WebOrigins { get; init; }

    public bool ServiceAccountsEnabled { get; init; } = false;

    [JsonPropertyName("secret")]
    public string? ClientSecret { get; init; }

    public Dictionary<string, string> Attributes { get; init; } = new Dictionary<string, string>();
}
