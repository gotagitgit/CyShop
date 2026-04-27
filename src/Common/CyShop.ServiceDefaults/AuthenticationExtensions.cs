using System.Security.Claims;
using Cyshop.Common.Models;
using CyShop.ServiceDefaults.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CyShop.ServiceDefaults;

public static class CurrentUserExtensions
{
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddScoped<CurrentUser>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());
        return services;
    }

    public static IApplicationBuilder UseCurrentUser(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CurrentUserMiddleware>();
    }
}

public static class AuthenticationExtensions
{
    public static IServiceCollection AddDefaultAuthentication(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        var identitySection = configuration.GetSection("Identity");

        if (!identitySection.Exists())
        {
            // Always register authentication and authorization even without identity config
            services.AddAuthentication();
            services.AddAuthorization();
            return services;
        }

        // prevent from mapping "sub" claim to nameidentifier.
        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
        {
            var identityUrl = identitySection["Url"] ?? throw new InvalidOperationException("Identity:Url is required");
            var audience = identitySection["Audience"] ?? throw new InvalidOperationException("Identity:Audience is required");

            options.Authority = identityUrl;
            options.RequireHttpsMetadata = false;
            options.Audience = audience;
            
#if DEBUG
            //Needed if using Android Emulator Locally. See https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/local-web-services?view=net-maui-8.0#android
            options.TokenValidationParameters.ValidIssuers = [identityUrl, "https://10.0.2.2:5243"];
#else
            options.TokenValidationParameters.ValidIssuers = [identityUrl];
#endif
            
            options.TokenValidationParameters.ValidateAudience = false;
        });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddDefaultCors(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });
        }
        else
        {
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.WithOrigins(allowedOrigins)
                          .WithHeaders("Authorization", "Content-Type")
                          .WithMethods("GET", "POST", "DELETE"));
            });
        }

        return builder.Services;
    }

    public static Guid? ResolveExternalId(this ClaimsPrincipal user, HttpContext httpContext)
    {
        var sub = user.FindFirstValue("sub");

        if (!string.IsNullOrEmpty(sub) && Guid.TryParse(sub, out var fromSub))
            return fromSub;

        if (user.HasClaim(c => c.Type == "scope" && c.Value.Contains("customers.api"))
            && httpContext.Request.Headers.TryGetValue("X-On-Behalf-Of", out var onBehalf)
            && Guid.TryParse(onBehalf.FirstOrDefault(), out var fromHeader))
            return fromHeader;

        return null;
    }
}
