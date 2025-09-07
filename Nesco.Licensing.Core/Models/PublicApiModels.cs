namespace Nesco.Licensing.Core.Models;

/// <summary>
/// Public API activation request
/// </summary>
public class ActivationRequest
{
    /// <summary>
    /// The license key (GUID)
    /// </summary>
    public Guid LicenseKey { get; set; }

    /// <summary>
    /// The secret key for license validation
    /// </summary>
    public string SecretKey { get; set; } = "";
}

/// <summary>
/// Public API deactivation request
/// </summary>
public class DeactivationRequest
{
    /// <summary>
    /// The license key (GUID)
    /// </summary>
    public Guid LicenseKey { get; set; }

    /// <summary>
    /// The secret key for license validation
    /// </summary>
    public string SecretKey { get; set; } = "";

    /// <summary>
    /// The specific activation ID to deactivate
    /// </summary>
    public Guid ActivationId { get; set; }
}


/// <summary>
/// Public API license data response
/// </summary>
public class LicenseDataResponse
{
    public Guid LicenseKey { get; set; }
    public string? ProductName { get; set; }
    public string? LicensedTo { get; set; }
    public string Status { get; set; } = "";
    public bool IsValid { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsExpired { get; set; }
    public string ExpiryType { get; set; } = "";
    public DateTime? ExpiryDate { get; set; }
    public int? ExpiryDays { get; set; }
    public int MaxActivations { get; set; }
    public int CurrentActivations { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? Params { get; set; }
    public List<LicenseActivationInfo> Activations { get; set; } = new();
}

/// <summary>
/// License activation information for public API
/// </summary>
public class LicenseActivationInfo
{
    public Guid ActivationId { get; set; }
    public string? MachineId { get; set; }
    public string? ClientType { get; set; }
    public string? AppVersion { get; set; }
    public DateTime ActivatedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Public API deactivation response
/// </summary>
public class DeactivationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public Guid LicenseKey { get; set; }
    public Guid ActivationId { get; set; }
    public DateTime DeactivatedAt { get; set; }
}

/// <summary>
/// Public API heartbeat response
/// </summary>
public class HeartbeatResponse
{
    public bool IsValid { get; set; }
    public bool IsActiveActivation { get; set; }
    public bool IsValidLicense { get; set; }
    public Guid ActivationId { get; set; }
    public Guid LicenseKey { get; set; }
    public string? ProductName { get; set; }
    public string? LicensedTo { get; set; }
    public string ExpiryType { get; set; } = "";
    public DateTime? ExpiryDate { get; set; }
    public int? ExpiryDays { get; set; }
    public DateTime ActivatedAt { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public string? Params { get; set; }
}

/// <summary>
/// Error response for public API
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = "";
}