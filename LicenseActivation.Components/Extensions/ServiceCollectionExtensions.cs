using LicenseActivation.Components.Core.Models;
using LicenseActivation.Components.Core.Services;
using LicenseActivation.Components.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace LicenseActivation.Components.Extensions;

/// <summary>
/// Extension methods for registering license activation services
/// </summary>
public static class ServiceCollectionExtensions
{
    
    /// <summary>
    /// Adds license activation component services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure the license activation settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLicenseActivationComponent(this IServiceCollection services, Action<LicenseActivationSettings> configureOptions)
    {
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        services.Configure<LicenseActivationSettings>(configureOptions);
        
        // Register HTTP client for the activation service
        services.AddHttpClient<ILicenseActivationService, HttpLicenseActivationService>();
        
        // Register browser fingerprinting service
        services.AddScoped<IBrowserFingerprintService, BrowserFingerprintService>();
        
        // Register client token service from Core
        // Use WebAssembly-compatible version when running in browser
        if (OperatingSystem.IsBrowser())
        {
            services.AddScoped<IClientTokenService>(sp =>
            {
                var jsRuntime = sp.GetService<IJSRuntime>();
                return new WebAssemblyClientTokenService(jsRuntime);
            });
        }
        else
        {
            services.AddScoped<IClientTokenService, ClientTokenService>();
        }
        
        return services;
    }
}