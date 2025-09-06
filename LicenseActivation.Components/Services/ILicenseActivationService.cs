using LicenseActivation.Components.Core.Models;

namespace LicenseActivation.Components.Services;

/// <summary>
/// Interface for license activation API operations
/// </summary>
public interface ILicenseActivationService
{
    /// <summary>
    /// Check for duplicate activations using machine fingerprint
    /// </summary>
    /// <param name="request">License activation request with fingerprint information</param>
    /// <returns>Duplicate check response indicating if license is already activated</returns>
    Task<DuplicateCheckResponse?> CheckDuplicateActivationAsync(LicenseActivationRequest request);
    
    /// <summary>
    /// Deactivate a license activation
    /// </summary>
    /// <param name="request">License deactivation request</param>
    /// <returns>Success indicator</returns>
    Task<bool> DeactivateLicenseAsync(LicenseDeactivationRequest request);
    
    /// <summary>
    /// Validate if a license is currently valid and active
    /// </summary>
    /// <param name="request">License validation request</param>
    /// <returns>License validation response with current status</returns>
    Task<LicenseValidationResponse?> ValidateLicenseAsync(LicenseValidationRequest request);
    
    /// <summary>
    /// Send heartbeat to check if license activation is still valid
    /// </summary>
    /// <param name="request">Secure heartbeat request with token containing activationId, customerEmail, and machineFingerprint</param>
    /// <param name="autoGenerateFingerprint">If true, automatically generates machine fingerprint and adds to request. Default is false.</param>
    /// <returns>Heartbeat response with license validity status</returns>
    Task<HeartbeatResponse?> SendHeartbeatAsync(HeartbeatRequest request);
    
    /// <summary>
    /// Activate license using a signed token through the service layer
    /// </summary>
    /// <param name="request">Token activation request</param>
    /// <returns>Token activation response</returns>
    Task<TokenActivationResponse> ActivateWithTokenAsync(TokenActivationRequest request);
    
}