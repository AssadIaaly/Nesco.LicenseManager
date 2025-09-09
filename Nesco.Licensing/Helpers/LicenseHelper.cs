using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Nesco.Licensing.Core.Models;
using Nesco.Licensing.Core.Services;
using Nesco.Licensing.Services;

namespace Nesco.Licensing.Helpers
{
    /// <summary>
    /// Helper class for license activation and heartbeat operations
    /// Can be used with DI or instantiated directly
    /// </summary>
    public class LicenseHelper
    {
        private readonly ILicenseActivationService _licenseService;
        private readonly IClientTokenService _clientTokenService;
        private readonly IBrowserFingerprintService _browserFingerprintService;
        private readonly LicenseActivationSettings? _settings;

        /// <summary>
        /// Constructor for DI usage
        /// </summary>
        public LicenseHelper(
            ILicenseActivationService licenseService,
            IClientTokenService clientTokenService,
            IBrowserFingerprintService browserFingerprintService,
            IOptions<LicenseActivationSettings>? settings = null)
        {
            _licenseService = licenseService;
            _clientTokenService = clientTokenService;
            _browserFingerprintService = browserFingerprintService;
            _settings = settings?.Value;
        }

        /// <summary>
        /// Activates a license using the provided token
        /// </summary>
        /// <param name="licenseToken">The license token to activate</param>
        /// <param name="productCode">The product code for validation</param>
        /// <param name="machineFingerprint">Optional: Custom machine fingerprint (if not provided, will be auto-generated)</param>
        /// <param name="eulaAcceptance">Optional: Pre-filled EULA acceptance information</param>
        /// <returns>Activation result with success status and activation details</returns>
        public async Task<ActivateLicenseResult> ActivateLicenseAsync(
            string licenseToken,
            string productCode,
            string? machineFingerprint = null,
            EulaAcceptanceInfo? eulaAcceptance = null)
        {
            var result = new ActivateLicenseResult();

            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(licenseToken))
                {
                    result.Success = false;
                    result.Error = "License token is required";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(productCode))
                {
                    result.Success = false;
                    result.Error = "Product code is required";
                    return result;
                }

                // Validate and decode token
                var tokenInfo = _clientTokenService.DecodeToken(licenseToken);
                if (tokenInfo == null)
                {
                    result.Success = false;
                    result.Error = "Invalid token format";
                    return result;
                }

                result.TokenInfo = tokenInfo;

                // Validate product code
                if (!_clientTokenService.ValidateProductCode(tokenInfo.ProductCode, productCode))
                {
                    result.Success = false;
                    result.Error = $"Token is not valid for product '{productCode}'. Token is for product '{tokenInfo.ProductCode}'";
                    return result;
                }

                // Check if token is expired
                if (_clientTokenService.IsTokenExpired(tokenInfo.ExpiryDate))
                {
                    result.Success = false;
                    result.Error = tokenInfo.ExpiryDate.HasValue
                        ? $"License token expired on {tokenInfo.ExpiryDate.Value:yyyy-MM-dd HH:mm}"
                        : "License token has expired";
                    return result;
                }

                // Validate signature if public key is available in settings
                var publicKey = _settings?.PublicKey;
                if (!string.IsNullOrEmpty(publicKey))
                {
                    if (!await _clientTokenService.ValidateTokenSignatureAsync(licenseToken, publicKey))
                    {
                        result.Success = false;
                        result.Error = "Invalid token signature";
                        return result;
                    }
                }

                // Generate or use provided machine fingerprint
                string finalMachineFingerprint = machineFingerprint ?? "";
                if (string.IsNullOrWhiteSpace(finalMachineFingerprint))
                {
                    var fingerprintResult = await _browserFingerprintService.GenerateFingerprintAsync();
                    finalMachineFingerprint = fingerprintResult.Hash;
                }

                // Prepare activation request
                var request = new TokenActivationRequest
                {
                    Token = licenseToken,
                    MachineFingerprint = finalMachineFingerprint,
                    EulaAcceptance = eulaAcceptance
                };

                // Attempt activation
                var activationResponse = await _licenseService.ActivateWithTokenAsync(request);

                if (activationResponse.Success)
                {
                    result.Success = true;
                    result.ActivationId = activationResponse.ActivationId;
                    result.ActivationResponse = activationResponse;
                    result.MachineFingerprint = finalMachineFingerprint;
                }
                else if (activationResponse.RequiredEula != null && 
                         !string.IsNullOrEmpty(activationResponse.Error) && 
                         activationResponse.Error.Contains("EULA acceptance required"))
                {
                    // EULA is required
                    result.Success = false;
                    result.RequiresEula = true;
                    result.RequiredEula = activationResponse.RequiredEula;
                    result.Error = activationResponse.Error;
                    result.MachineFingerprint = finalMachineFingerprint;
                }
                else
                {
                    result.Success = false;
                    result.Error = activationResponse.Error ?? "Activation failed";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = $"Activation error: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Sends a heartbeat to check license activation validity
        /// </summary>
        /// <param name="activationId">The activation ID to check</param>
        /// <param name="customerEmail">Optional: Customer email for validation (defaults to "test@example.com" if not provided)</param>
        /// <param name="machineFingerprint">Optional: Machine fingerprint (if not provided, will be auto-generated)</param>
        /// <returns>Heartbeat result with validation status and license details</returns>
        public async Task<HeartbeatResult> SendHeartbeatAsync(
            Guid activationId,
            string? customerEmail = null,
            string? machineFingerprint = null)
        {
            var result = new HeartbeatResult
            {
                ActivationId = activationId,
                CustomerEmail = customerEmail
            };

            try
            {
                // Validate inputs
                if (activationId == Guid.Empty)
                {
                    result.Success = false;
                    result.Error = "Invalid activation ID";
                    result.ErrorCode = "INVALID_ID";
                    return result;
                }

                // Use provided email or default
                var finalEmail = string.IsNullOrWhiteSpace(customerEmail) ? "test@example.com" : customerEmail.Trim();

                // Generate or use provided machine fingerprint
                string finalMachineFingerprint = machineFingerprint ?? "";
                if (string.IsNullOrWhiteSpace(finalMachineFingerprint))
                {
                    var fingerprintResult = await _browserFingerprintService.GenerateFingerprintAsync();
                    finalMachineFingerprint = fingerprintResult.Hash;
                }

                result.MachineFingerprint = finalMachineFingerprint;

                // Create heartbeat request
                var heartbeatRequest = new HeartbeatRequest(activationId, finalEmail, finalMachineFingerprint);
                result.SecurityToken = heartbeatRequest.Token;

                // Send heartbeat
                var response = await _licenseService.SendHeartbeatAsync(heartbeatRequest);

                if (response != null)
                {
                    result.Success = response.IsValid;
                    result.Response = response;
                    
                    if (!response.IsValid)
                    {
                        result.Error = "License validation failed";
                        result.ErrorCode = "VALIDATION_FAILED";
                    }
                }
                else
                {
                    result.Success = false;
                    result.Error = "No response received from server";
                    result.ErrorCode = "NO_RESPONSE";
                }
            }
            catch (HttpRequestException httpEx)
            {
                result.Success = false;
                result.Error = httpEx.Message;
                
                // Parse error code from message format: "StatusCode|ErrorMessage"
                if (httpEx.Message.Contains("|"))
                {
                    var parts = httpEx.Message.Split('|', 2);
                    var statusCode = parts[0];
                    var apiError = parts.Length > 1 ? parts[1] : "";
                    
                    result.ErrorCode = statusCode;
                    result.ErrorDetails = apiError;
                    
                    // Set specific error messages based on status code
                    if (statusCode == "401")
                    {
                        if (apiError.Contains("The license does not belong to this customer"))
                        {
                            result.Error = "Customer email mismatch - security validation failed";
                            result.ErrorCode = "CUSTOMER_MISMATCH";
                        }
                        else if (apiError.Contains("The license does not belong to this machine"))
                        {
                            result.Error = "Machine fingerprint mismatch - security validation failed";
                            result.ErrorCode = "MACHINE_MISMATCH";
                        }
                        else
                        {
                            result.Error = "Security validation failed";
                            result.ErrorCode = "UNAUTHORIZED";
                        }
                    }
                    else if (statusCode == "404")
                    {
                        if (apiError.Contains("Activation not found"))
                        {
                            result.Error = "Activation ID not found";
                            result.ErrorCode = "ACTIVATION_NOT_FOUND";
                        }
                        else if (apiError.Contains("License not found"))
                        {
                            result.Error = "License not found for this activation";
                            result.ErrorCode = "LICENSE_NOT_FOUND";
                        }
                        else
                        {
                            result.Error = "Resource not found";
                            result.ErrorCode = "NOT_FOUND";
                        }
                    }
                    else if (statusCode == "400")
                    {
                        result.Error = "Bad request - validation failed";
                        result.ErrorCode = "BAD_REQUEST";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = $"Heartbeat error: {ex.Message}";
                result.ErrorCode = "EXCEPTION";
            }

            return result;
        }

        /// <summary>
        /// Static factory method to create a standalone instance without DI
        /// </summary>
        /// <param name="apiBaseUrl">The API base URL for the license server</param>
        /// <param name="publicKey">Optional: RSA public key for signature validation</param>
        /// <returns>A configured LicenseHelper instance</returns>
        public static LicenseHelper CreateStandalone(string apiBaseUrl, string? publicKey = null)
        {
            var settings = Options.Create(new LicenseActivationSettings
            {
                ApiBaseUrl = apiBaseUrl,
                PublicKey = publicKey
            });

            var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
            var licenseService = new HttpLicenseActivationService(httpClient, settings);
            var clientTokenService = new ClientTokenService();
            var browserFingerprintService = new BrowserFingerprintService(null!); // Will work for generating fingerprints

            return new LicenseHelper(licenseService, clientTokenService, browserFingerprintService, settings);
        }
    }

    /// <summary>
    /// Result of a license activation attempt
    /// </summary>
    public class ActivateLicenseResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public Guid? ActivationId { get; set; }
        public ClientLicenseTokenData? TokenInfo { get; set; }
        public TokenActivationResponse? ActivationResponse { get; set; }
        public bool RequiresEula { get; set; }
        public EulaInfo? RequiredEula { get; set; }
        public string? MachineFingerprint { get; set; }
    }

    /// <summary>
    /// Result of a heartbeat check
    /// </summary>
    public class HeartbeatResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorDetails { get; set; }
        public Guid ActivationId { get; set; }
        public string? CustomerEmail { get; set; }
        public string? MachineFingerprint { get; set; }
        public string? SecurityToken { get; set; }
        public HeartbeatResponse? Response { get; set; }
    }
}