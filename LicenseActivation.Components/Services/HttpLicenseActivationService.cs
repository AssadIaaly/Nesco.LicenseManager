using System.Net.Http.Json;
using System.Text.Json;
using LicenseActivation.Components.Core.Models;
using Microsoft.Extensions.Options;

namespace LicenseActivation.Components.Services;

/// <summary>
/// HTTP-based implementation of ILicenseActivationService
/// </summary>
public class HttpLicenseActivationService : ILicenseActivationService
{
    private readonly HttpClient _httpClient;
    private readonly LicenseActivationSettings _settings;
    private readonly IBrowserFingerprintService? _fingerprintService;

    public HttpLicenseActivationService(HttpClient httpClient, IOptions<LicenseActivationSettings> settings, IBrowserFingerprintService? fingerprintService = null)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _fingerprintService = fingerprintService;
        
        // Set base address if configured
        if (!string.IsNullOrEmpty(_settings.ApiBaseUrl) && _httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_settings.ApiBaseUrl);
        }
    }

    public async Task<DuplicateCheckResponse?> CheckDuplicateActivationAsync(LicenseActivationRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/license/check-duplicate", request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DuplicateCheckResponse>(responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }
    
    public async Task<bool> DeactivateLicenseAsync(LicenseDeactivationRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/license/deactivate", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<LicenseValidationResponse?> ValidateLicenseAsync(LicenseValidationRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/license/validate", request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<LicenseValidationResponse>(responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<HeartbeatResponse?> SendHeartbeatAsync(HeartbeatRequest request)
    {
        try
        {
            // Auto-generate fingerprint if requested
            if (request.MachineFingerprint == string.Empty && _fingerprintService != null)
            {
                var fingerprintResult = await _fingerprintService.GenerateFingerprintAsync();
                
                // Parse the existing token to get activationId and customerEmail
                var existingToken = HeartbeatToken.FromToken(request.Token);
                if (existingToken != null)
                {
                    // Create new request with auto-generated fingerprint
                    request = new HeartbeatRequest(existingToken.ActivationId, existingToken.CustomerEmail, fingerprintResult.Hash);
                }
            }
            
            var response = await _httpClient.PostAsJsonAsync("/api/license/heartbeat", request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<HeartbeatResponse>(responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            
            // For non-success status codes, throw an exception with details
            var errorContent = await response.Content.ReadAsStringAsync();
            string errorMessage = "Unknown error";
            
            try
            {
                // Try to parse error response as JSON
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                errorMessage = errorResponse?.Error ?? errorContent;
            }
            catch
            {
                // If not JSON, use raw content
                errorMessage = string.IsNullOrEmpty(errorContent) ? response.ReasonPhrase ?? "Unknown error" : errorContent;
            }
            
            // Throw HttpRequestException with status code and error message
            throw new HttpRequestException($"{(int)response.StatusCode}|{errorMessage}", null, response.StatusCode);
        }
        catch (HttpRequestException)
        {
            // Re-throw HTTP exceptions
            throw;
        }
        catch (Exception ex)
        {
            // Wrap other exceptions
            throw new HttpRequestException($"Network error: {ex.Message}", ex);
        }
    }
    public async Task<TokenActivationResponse> ActivateWithTokenAsync(TokenActivationRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/license/activate", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenActivationResponse>();
                return result ?? new TokenActivationResponse { Success = false, Error = "Invalid response from server" };
            }
            
            // Try to parse error response
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<TokenActivationResponse>();
                return errorResponse ?? new TokenActivationResponse { Success = false, Error = $"Activation failed: {response.StatusCode}" };
            }
            catch
            {
                return new TokenActivationResponse { Success = false, Error = $"Activation failed: {response.StatusCode}" };
            }
        }
        catch (Exception ex)
        {
            return new TokenActivationResponse { Success = false, Error = $"Network error: {ex.Message}" };
        }
    }
    
    
}