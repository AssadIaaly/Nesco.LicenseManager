namespace Nesco.Licensing.Core.Models;

/// <summary>
/// License activation request for API calls
/// </summary>
public class LicenseActivationRequest
{
    public Guid LicenseKey { get; set; }
    public string SecretKey { get; set; } = "";
    public string MachineId { get; set; } = "";
    public string? ApplicationVersion { get; set; }
    public string? ClientType { get; set; }
    public string? MachineFingerprint { get; set; }
    public bool ForceDuplicateActivation { get; set; } = false;
}


/// <summary>
/// License deactivation request for API calls
/// </summary>
public class LicenseDeactivationRequest
{
    public Guid LicenseKey { get; set; }
    public string SecretKey { get; set; } = "";
    public string MachineId { get; set; } = "";
}

/// <summary>
/// License validation request for API calls
/// </summary>
public class LicenseValidationRequest
{
    public Guid LicenseKey { get; set; }
    public string SecretKey { get; set; } = "";
}

/// <summary>
/// License validation response from API
/// </summary>
public class LicenseValidationResponse
{
    public bool IsValid { get; set; }
    public Guid LicenseKey { get; set; }
    public string? ProductName { get; set; }
    public string? LicensedTo { get; set; }
    public string Status { get; set; } = "";
    public LicenseExpiryType ExpiryType { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? ExpiryDays { get; set; }
    public int MaxActivations { get; set; }
    public int CurrentActivations { get; set; }
    public string? Params { get; set; }
}

/// <summary>
/// Duplicate check response from API
/// </summary>
public class DuplicateCheckResponse
{
    public bool HasDuplicate { get; set; }
    public string ClientType { get; set; } = "";
    public string MachineFingerprint { get; set; } = "";
    public ActivationInfo? ExistingActivation { get; set; }
}

/// <summary>
/// Existing activation information
/// </summary>
public class ActivationInfo
{
    public DateTime ActivatedAt { get; set; }
    public string? MachineId { get; set; }
    public string? ClientType { get; set; }
    public string? AppVersion { get; set; }
}