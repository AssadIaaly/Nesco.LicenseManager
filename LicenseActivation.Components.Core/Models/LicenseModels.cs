using System;

namespace LicenseActivation.Components.Core.Models;

/// <summary>
/// License file data structure (JSON format)
/// </summary>
public class LicenseFileData
{
    public Guid LicenseKey { get; set; }
    public string SecretKey { get; set; } = "";
    public string? Params { get; set; }
}

/// <summary>
/// Simplified license file format for API file upload
/// </summary>
public class SimpleLicenseFile
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
/// License expiry type enumeration
/// </summary>
public enum LicenseExpiryType
{
    Perpetual = 0,          // Never expires
    DaysFromActivation = 1, // Expires X days after activation
    FixedDate = 2           // Expires on a specific date
}