using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace LicenseActivation.Components.Services;

/// <summary>
/// Service for browser fingerprinting functionality
/// </summary>
public interface IBrowserFingerprintService
{
    /// <summary>
    /// Generate a comprehensive browser fingerprint
    /// </summary>
    Task<BrowserFingerprintResult> GenerateFingerprintAsync();

}

/// <summary>
/// Implementation of browser fingerprinting service
/// </summary>
public class BrowserFingerprintService(IJSRuntime jsRuntime) : IBrowserFingerprintService
{

   

    public async Task<BrowserFingerprintResult> GenerateFingerprintAsync()
    {
        
        try
        {
            var result = await jsRuntime.InvokeAsync<BrowserFingerprintResult>("browserFingerprint.generateFingerprint");
            return result;
        }
        catch (Exception)
        {
            // Return a basic fingerprint if the advanced one fails
            return new BrowserFingerprintResult
            {
                Hash = Guid.NewGuid().ToString("N"),
                Details = new { basic = true }
            };
        }
    }
    
}

/// <summary>
/// Result of browser fingerprinting
/// </summary>
public class BrowserFingerprintResult
{
    public string Hash { get; set; } = "";
    public object? Details { get; set; }
    public string Raw { get; set; } = "";
}