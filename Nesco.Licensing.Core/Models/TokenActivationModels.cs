namespace Nesco.Licensing.Core.Models;

/// <summary>
/// Request model for token-based license activation
/// </summary>
public class TokenActivationRequest
{
    /// <summary>
    /// The JWT-like license token containing all license information
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// The machine fingerprint for this activation
    /// </summary>
    public string MachineFingerprint { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional machine name for identification
    /// </summary>
    public string? MachineName { get; set; }
    
    /// <summary>
    /// Optional operating system information
    /// </summary>
    public string? OperatingSystem { get; set; }
    
    /// <summary>
    /// Optional application version
    /// </summary>
    public string? ApplicationVersion { get; set; }
    
    /// <summary>
    /// EULA acceptance information
    /// </summary>
    public EulaAcceptanceInfo? EulaAcceptance { get; set; }
}

/// <summary>
/// Response model for token-based activation
/// </summary>
public class TokenActivationResponse
{
    /// <summary>
    /// Whether the activation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// The activation ID for heartbeat checks
    /// </summary>
    public Guid? ActivationId { get; set; }
    
    /// <summary>
    /// The product code from the token
    /// </summary>
    public string? ProductCode { get; set; }
    
    /// <summary>
    /// License expiry date (if applicable)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    
    /// <summary>
    /// Any additional parameters from the license
    /// </summary>
    public string? Params { get; set; }
    
    /// <summary>
    /// Error message if activation failed
    /// </summary>
    public string? Error { get; set; }
    
    /// <summary>
    /// Activation timestamp
    /// </summary>
    public DateTime? ActivatedAt { get; set; }
    
    /// <summary>
    /// EULA information that needs to be accepted
    /// </summary>
    public EulaInfo? RequiredEula { get; set; }
}

/// <summary>
/// EULA information for activation
/// </summary>
public class EulaInfo
{
    /// <summary>
    /// EULA ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// EULA name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// EULA content
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// EULA version
    /// </summary>
    public string Version { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the content is HTML formatted
    /// </summary>
    public bool IsHtmlContent { get; set; } = false;
}

/// <summary>
/// EULA acceptance information for activation
/// </summary>
public class EulaAcceptanceInfo
{
    /// <summary>
    /// ID of the EULA being accepted
    /// </summary>
    public int EulaId { get; set; }
    
    /// <summary>
    /// Name of the person accepting the EULA
    /// </summary>
    public string AcceptedByName { get; set; } = string.Empty;
    
    /// <summary>
    /// Email of the person accepting the EULA
    /// </summary>
    public string AcceptedByEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the user has accepted the EULA
    /// </summary>
    public bool IsAccepted { get; set; }
}