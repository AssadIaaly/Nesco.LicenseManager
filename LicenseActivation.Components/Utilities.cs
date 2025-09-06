using LicenseActivation.Components.Core.Models;
using LicenseActivation.Components.Core.Services;
using LicenseActivation.Components.Services;

namespace LicenseActivation.Components
{
    public static class ActivationUtilities
    {
        /// <summary>
        /// Validates the license token before activation
        /// </summary>
        public static async Task<(bool isValid, string? errorMessage, ClientLicenseTokenData? tokenData)> ValidateLicenseToken(
            string licenseToken,
            string productCode,
            IClientTokenService clientTokenService,
            string? publicKey = null)
        {
            // Decode and validate token format
            var tokenData = clientTokenService.DecodeToken(licenseToken);
            if (tokenData == null)
            {
                return (false, "Invalid token format", null);
            }

            // Validate product code
            if (!clientTokenService.ValidateProductCode(tokenData.ProductCode, productCode))
            {
                return (false, $"Token is not valid for product '{productCode}'. Token is for product '{tokenData.ProductCode}'", null);
            }

            // Check if token is expired
            if (clientTokenService.IsTokenExpired(tokenData.ExpiryDate))
            {
                var expiryMsg = tokenData.ExpiryDate.HasValue
                    ? $"License token expired on {tokenData.ExpiryDate.Value:yyyy-MM-dd HH:mm}"
                    : "License token has expired";
                return (false, expiryMsg, null);
            }

            // Validate signature if public key is available
            if (!string.IsNullOrEmpty(publicKey))
            {
                if (! await clientTokenService.ValidateTokenSignatureAsync(licenseToken, publicKey))
                {
                    return (false, "Invalid token signature", null);
                }
            }

            return (true, null, tokenData);
        }

        /// <summary>
        /// Validates EULA acceptance fields
        /// </summary>
        public static (bool isValid, string? errorMessage) ValidateEulaAcceptance(
            string? acceptedByName,
            string? acceptedByEmail,
            bool isAccepted,
            string? parameterName = null,
            string? parameterEmail = null)
        {
            // Use parameter values if provided, otherwise use form values
            var finalName = !string.IsNullOrEmpty(parameterName) ? parameterName : acceptedByName;
            var finalEmail = !string.IsNullOrEmpty(parameterEmail) ? parameterEmail : acceptedByEmail;

            if (string.IsNullOrWhiteSpace(finalName))
            {
                return (false, "Name is required for EULA acceptance");
            }

            if (string.IsNullOrWhiteSpace(finalEmail))
            {
                return (false, "Email is required for EULA acceptance");
            }

            if (!isAccepted)
            {
                return (false, "You must accept the EULA to continue");
            }

            return (true, null);
        }

        /// <summary>
        /// Gets the final values for EULA acceptance, prioritizing parameters over form values
        /// </summary>
        public static (string name, string email) GetFinalEulaValues(
            string? formName,
            string? formEmail,
            string? parameterName,
            string? parameterEmail)
        {
            var finalName = !string.IsNullOrEmpty(parameterName) ? parameterName : formName ?? "";
            var finalEmail = !string.IsNullOrEmpty(parameterEmail) ? parameterEmail : formEmail ?? "";
            return (finalName, finalEmail);
        }

        /// <summary>
        /// Determines if EULA response indicates that EULA acceptance is required
        /// </summary>
        public static bool IsEulaRequired(TokenActivationResponse result)
        {
            return result.RequiredEula != null && 
                   !string.IsNullOrEmpty(result.Error) && 
                   result.Error.Contains("EULA acceptance required");
        }
    }
}