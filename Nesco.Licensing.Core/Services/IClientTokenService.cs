namespace Nesco.Licensing.Core.Services;

public interface IClientTokenService
{
    ClientLicenseTokenData? DecodeToken(string token);
    Task<bool> ValidateTokenSignatureAsync(string token, string publicKey);
    bool ValidateProductCode(string tokenProductCode, string expectedProductCode);
    bool IsTokenExpired(DateTime? expiryDate);
}

/// <summary>
/// Client-side representation of license token data
/// </summary>
public class ClientLicenseTokenData
{
    public string ProductCode { get; set; } = string.Empty;
    public Guid LicenseKey { get; set; }
    public string SecretKey { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public string Signature { get; set; } = string.Empty;
    public string? Params { get; set; }
    
    public string GetDataForSigning()
    {
        var expiryString = ExpiryDate?.ToString("O") ?? "perpetual";
        return $"{ProductCode}.{LicenseKey}.{SecretKey}.{expiryString}";
    }
}