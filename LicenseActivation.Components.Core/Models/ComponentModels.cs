using System;

namespace LicenseActivation.Components.Core.Models;

/// <summary>
/// Result of license activation operation (used by components)
/// </summary>
public class LicenseActivationResult
{
    public bool Success { get; set; }
    public string LicenseKey { get; set; } = "";
    public Guid ActivationId { get; set; }
    public string? ProductName { get; set; }
    public string? LicensedTo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Params { get; set; }
    public string Response { get; set; } = "";
}

/// <summary>
/// Message to be shown to the user
/// </summary>
public class LicenseActivationMessage
{
    public string Message { get; set; } = "";
    public LicenseActivationMessageType Type { get; set; }
}

/// <summary>
/// Types of messages that can be shown
/// </summary>
public enum LicenseActivationMessageType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// History item for tracking activation attempts
/// </summary>
public class ActivationHistoryItem
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = "";
    public string LicenseKey { get; set; } = "";
    public string MachineId { get; set; } = "";
    public bool Success { get; set; }
}

/// <summary>
/// Configuration settings for license activation component
/// </summary>
public class LicenseActivationSettings
{
    public string ApiBaseUrl { get; set; } = "";
    
    /// <summary>
    /// Public key used for token signature validation
    /// </summary>
    public string? PublicKey { get; set; }
}