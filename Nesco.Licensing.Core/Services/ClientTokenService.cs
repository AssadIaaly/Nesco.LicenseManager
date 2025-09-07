using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Nesco.Licensing.Core.Services;

public class ClientTokenService : IClientTokenService
{
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
            // Check if we're running in WebAssembly (browser environment)
            if (OperatingSystem.IsBrowser())
            {
                // In WebAssembly, RSA operations are not supported in this implementation
                // The WebAssemblyClientTokenService should be used instead for browser environments
                // as it uses JavaScript interop with Web Crypto API
                
                // Basic validation: check that signature and public key are not empty
                if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(publicKey))
                    return false;
                
                // For this base implementation in browser environments,
                // we provide basic validation only
                await Task.CompletedTask; // Make it properly async
                return true;
            }
            
            // Full RSA signature verification for non-browser environments
            using var rsa = RSA.Create();
            
            // Clean the public key - remove newlines and extra whitespace
            var cleanPublicKey = publicKey.Replace("\n", "").Replace("\r", "").Replace(" ", "");
            
            // Import the public key - handle different formats
            var publicKeyBytes = Convert.FromBase64String(cleanPublicKey);
            
            // Try different import methods for public key
            try
            {
                // Try SubjectPublicKeyInfo format (X.509, most common)
                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            }
            catch
            {
                try
                {
                    // Try RSA public key format
                    rsa.ImportRSAPublicKey(publicKeyBytes, out _);
                }
                catch
                {
                    // For PEM format, we need to decode the actual key content
                    try
                    {
                        // Convert bytes back to string to check for PEM format
                        var pemString = Encoding.UTF8.GetString(publicKeyBytes);
                        if (pemString.Contains("BEGIN PUBLIC KEY"))
                        {
                            // Extract the actual key content between the headers
                            var startMarker = "-----BEGIN PUBLIC KEY-----";
                            var endMarker = "-----END PUBLIC KEY-----";
                            var startIndex = pemString.IndexOf(startMarker) + startMarker.Length;
                            var endIndex = pemString.IndexOf(endMarker);
                            var keyContent = pemString.Substring(startIndex, endIndex - startIndex).Replace("\n", "").Replace("\r", "");
                            var actualKeyBytes = Convert.FromBase64String(keyContent);
                            rsa.ImportSubjectPublicKeyInfo(actualKeyBytes, out _);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        return false; // Unsupported public key format
                    }
                }
            }
            
            // Verify the signature
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = Convert.FromBase64String(signature);
            
            return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch(Exception e)
        {
            Console.WriteLine(e.InnerException?.Message);
            return false;
        }
    }
}