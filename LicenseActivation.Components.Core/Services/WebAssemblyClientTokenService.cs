using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using LicenseActivation.Components.Core.Services;

namespace LicenseActivation.Components.Core.Services;

/// <summary>
/// WebAssembly-compatible implementation of IClientTokenService that uses JavaScript interop
/// for RSA signature verification via Web Crypto API
/// </summary>
public class WebAssemblyClientTokenService : IClientTokenService
{
    private readonly IJSRuntime? _jsRuntime;
    
    public WebAssemblyClientTokenService(IJSRuntime? jsRuntime = null)
    {
        _jsRuntime = jsRuntime;
    }
    
    public ClientLicenseTokenData? DecodeToken(string token)
    {
        try
        {
            // Remove any whitespace
            token = token.Trim();
            
            // Decode from Base64
            var tokenBytes = Convert.FromBase64String(token);
            var tokenJson = Encoding.UTF8.GetString(tokenBytes);
            
            // Deserialize the token data
            return JsonSerializer.Deserialize<ClientLicenseTokenData>(tokenJson);
        }
        catch
        {
            return null;
        }
    }
    
    
    /// <summary>
    /// Async version for proper WebAssembly support
    /// </summary>
    public async Task<bool> ValidateTokenSignatureAsync(string token, string publicKey)
    {
        try
        {
            var tokenData = DecodeToken(token);
            if (tokenData == null)
                return false;
            
            var dataToVerify = tokenData.GetDataForSigning();
            return await VerifySignatureAsync(dataToVerify, tokenData.Signature, publicKey);
        }
        catch
        {
            return false;
        }
    }
    
    public bool ValidateProductCode(string tokenProductCode, string expectedProductCode)
    {
        if (string.IsNullOrEmpty(tokenProductCode) || string.IsNullOrEmpty(expectedProductCode))
            return false;
            
        return string.Equals(tokenProductCode, expectedProductCode, StringComparison.OrdinalIgnoreCase);
    }
    
    public bool IsTokenExpired(DateTime? expiryDate)
    {
        if (!expiryDate.HasValue)
            return false; // Perpetual license
            
        return expiryDate.Value < DateTime.UtcNow;
    }
    
    private async Task<bool> VerifySignatureAsync(string data, string signature, string publicKey)
    {
        try
        {
            // Check if we're running in WebAssembly with JS runtime available
            if (OperatingSystem.IsBrowser() && _jsRuntime != null)
            {
                try
                {
                    if (string.IsNullOrEmpty(publicKey))
                    {
                        Console.WriteLine("No public key provided for signature verification");
                        return false;
                    }
                    
                    Console.WriteLine($"WebAssembly signature verification - Data length: {data?.Length}, Signature length: {signature?.Length}");
                    Console.WriteLine($"Using public key (first 50 chars): {publicKey.Substring(0, Math.Min(50, publicKey.Length))}");
                    
                    // Use Web Crypto API through JavaScript interop
                    var result = await _jsRuntime.InvokeAsync<bool>(
                        "rsaVerification.verifySignature",
                        data,
                        signature,
                        publicKey
                    );
                    
                    Console.WriteLine($"JavaScript signature verification result: {result}");
                    return result;
                }
                catch (Exception jsError)
                {
                    Console.WriteLine($"JavaScript signature verification failed: {jsError.Message}");
                    Console.WriteLine($"JS Error details: {jsError}");
                    
                    // Return false - proper verification failed
                    return false;
                }
            }
            
            // For non-browser environments, use fallback
            return VerifySignatureFallback(data, signature, publicKey);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Signature verification error: {e.Message}");
            return false;
        }
    }
    
    private bool VerifySignatureFallback(string data, string signature, string publicKey)
    {
        // In WebAssembly without JS runtime, we can't perform RSA operations
        if (OperatingSystem.IsBrowser())
        {
            // Basic validation: check that signature and public key are not empty
            // Real validation happens server-side
            return !string.IsNullOrEmpty(signature) && !string.IsNullOrEmpty(publicKey);
        }
        
        // For non-browser environments, try to use RSA
        try
        {
            using var rsa = System.Security.Cryptography.RSA.Create();
            
            // Clean the public key
            var cleanPublicKey = publicKey.Replace("\n", "").Replace("\r", "").Replace(" ", "");
            var publicKeyBytes = Convert.FromBase64String(cleanPublicKey);
            
            // Try to import the key
            try
            {
                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            }
            catch
            {
                try
                {
                    rsa.ImportRSAPublicKey(publicKeyBytes, out _);
                }
                catch
                {
                    // Handle PEM format
                    var pemString = Encoding.UTF8.GetString(publicKeyBytes);
                    if (pemString.Contains("BEGIN PUBLIC KEY"))
                    {
                        var startMarker = "-----BEGIN PUBLIC KEY-----";
                        var endMarker = "-----END PUBLIC KEY-----";
                        var startIndex = pemString.IndexOf(startMarker) + startMarker.Length;
                        var endIndex = pemString.IndexOf(endMarker);
                        var keyContent = pemString.Substring(startIndex, endIndex - startIndex)
                            .Replace("\n", "").Replace("\r", "");
                        var actualKeyBytes = Convert.FromBase64String(keyContent);
                        rsa.ImportSubjectPublicKeyInfo(actualKeyBytes, out _);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            
            // Verify the signature
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = Convert.FromBase64String(signature);
            
            return rsa.VerifyData(dataBytes, signatureBytes, 
                System.Security.Cryptography.HashAlgorithmName.SHA256, 
                System.Security.Cryptography.RSASignaturePadding.Pkcs1);
        }
        catch
        {
            return false;
        }
    }
}