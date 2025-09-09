# 🔐 LicenseActivation.Components

[![NuGet Version](https://img.shields.io/nuget/v/Nesco.LicenseActivation.Blazor)](https://www.nuget.org/packages/Nesco.Licensing/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly%20%7C%20Server-512BD4)](https://blazor.net)

Blazor component library for license activation with browser fingerprinting, EULA support, and token-based activation.

## Features

- **MudLicenseActivationComponent** - MudBlazor-based with Material Design
- **HtmlLicenseActivationComponent** - Lightweight HTML/CSS implementation  
- Machine fingerprinting for device identification (consistent across browsers)
- EULA acceptance workflow with dialog presentation
- Two-step activation process (attempts without EULA first, then with EULA if required)
- Parameter support for pre-filled EULA acceptance

## Installation

```bash
dotnet add package Nesco.Licensing
```

## Setup

### 1. Add to Program.cs

```csharp
using LicenseActivation.Components.Extensions;
using MudBlazor.Services; // Only if using MudLicenseActivationComponent

// Add MudBlazor (only if using MudLicenseActivationComponent)
builder.Services.AddMudServices();

// Add License Activation services
builder.Services.AddNescoLicensing(options =>
{
    options.ApiBaseUrl = "https://your-api-url.com";
    options.PublicKey = "your-rsa-public-key"; // Optional for signature validation
});
```

### 2. Add to App.razor

```html
<!-- Add machine fingerprinting script -->
<script src="_content/LicenseActivation.Components/js/lic.min.js"></script>
```

## Component Usage

### MudLicenseActivationComponent (requires MudBlazor)

```razor
@page "/license"
@using LicenseActivation.Components

<MudLicenseActivationComponent 
    ProductCode="YOUR_PRODUCT_CODE"
    AcceptedByName="@userName"       @* Optional: Pre-fill EULA name *@
    AcceptedByEmail="@userEmail"     @* Optional: Pre-fill EULA email *@
    MachineFingerprint="@machineId"  @* Optional: Override machine fingerprint *@
    OnActivationSuccess="OnSuccess"
    OnActivationError="OnError" />

@code {
    private async Task OnSuccess(TokenActivationResponse result)
    {
        // Handle successful activation
    }
    
    private async Task OnError(string error)
    {
        // Handle error
    }
}
```

### HtmlLicenseActivationComponent (no dependencies)

```razor
@page "/license"
@using LicenseActivation.Components

<HtmlLicenseActivationComponent 
    ProductCode="YOUR_PRODUCT_CODE"
    AcceptedByName="@userName"       @* Optional: Pre-fill EULA name *@
    AcceptedByEmail="@userEmail"     @* Optional: Pre-fill EULA email *@
    MachineFingerprint="@machineId"  @* Optional: Override machine fingerprint *@
    OnActivationSuccess="OnSuccess"
    OnActivationError="OnError" />
```

### HeartbeatTestComponent (testing utility)

```razor
@page "/heartbeat-test"
@using LicenseActivation.Components

<HeartbeatTestComponent />
```

This component provides a complete testing interface for heartbeat functionality with:
- Input fields for activation ID, customer email, and machine fingerprint
- Auto-generation of machine fingerprints when field is left empty
- Real-time heartbeat testing with detailed results
- Security token visualization
- Response analysis and validation status display
- Copy functionality for sharing test results

## Component Parameters

| Parameter             | Type                                     | Description                                                                                  |
|-----------------------|------------------------------------------|----------------------------------------------------------------------------------------------|
| `ProductCode`         | `string`                                 | **Required**. Product code for license validation                                            |
| `AcceptedByName`      | `string?`                                | Pre-fill name for EULA acceptance                                                            |
| `AcceptedByEmail`     | `string?`                                | Pre-fill email for EULA acceptance                                                           |
| `MachineFingerprint`  | `string?`                                | Override machine fingerprint. When provided, disables manual input and skips auto-generation |
| `OnActivationSuccess` | `EventCallback<TokenActivationResponse>` | Success callback                                                                             |
| `OnActivationError`   | `EventCallback<string>`                  | Error callback                                                                               |

## Helper Methods (Nesco.Licensing.Helpers)

The library includes a `LicenseHelper` class for programmatic license activation and heartbeat operations. It can be used with dependency injection or as a standalone instance.

### Using with Dependency Injection

When using `AddNescoLicensing()`, the `LicenseHelper` is automatically registered in the DI container:

```csharp
@inject LicenseHelper LicenseHelper

// Or in a service/controller:
public class MyService
{
    private readonly LicenseHelper _licenseHelper;
    
    public MyService(LicenseHelper licenseHelper)
    {
        _licenseHelper = licenseHelper;
    }
}
```

### Using as Standalone (without DI)

Create a standalone instance for console apps or scenarios without DI:

```csharp
// Create standalone helper
var licenseHelper = LicenseHelper.CreateStandalone(
    apiBaseUrl: "https://your-api-url.com",
    publicKey: "your-rsa-public-key" // Optional
);
```

### ActivateLicenseAsync

Activates a license with full validation and EULA support:

```csharp
using Nesco.Licensing.Helpers;
using Nesco.Licensing.Core.Models;

// Basic activation (auto-generates machine fingerprint)
var result = await licenseHelper.ActivateLicenseAsync(
    licenseToken: "your-license-token",
    productCode: "YOUR_PRODUCT_CODE"
);

if (result.Success)
{
    Console.WriteLine($"License activated! Activation ID: {result.ActivationId}");
}
else if (result.RequiresEula)
{
    // EULA acceptance is required
    Console.WriteLine($"EULA required: {result.RequiredEula?.Name}");
    
    // Retry with EULA acceptance
    var eulaAcceptance = new EulaAcceptanceInfo
    {
        EulaId = result.RequiredEula.Id,
        AcceptedByName = "John Doe",
        AcceptedByEmail = "john@example.com",
        IsAccepted = true
    };
    
    result = await licenseHelper.ActivateLicenseAsync(
        licenseToken: "your-license-token",
        productCode: "YOUR_PRODUCT_CODE",
        eulaAcceptance: eulaAcceptance
    );
}
else
{
    Console.WriteLine($"Activation failed: {result.Error}");
}

// With custom machine fingerprint
var result = await licenseHelper.ActivateLicenseAsync(
    licenseToken: "your-license-token",
    productCode: "YOUR_PRODUCT_CODE",
    machineFingerprint: "custom-machine-id"
);
```

### SendHeartbeatAsync

Sends a heartbeat to verify license validity:

```csharp
using Nesco.Licensing.Helpers;

// Send heartbeat (auto-generates machine fingerprint)
var heartbeatResult = await licenseHelper.SendHeartbeatAsync(
    activationId: myActivationId,
    customerEmail: "customer@example.com" // Optional
);

if (heartbeatResult.Success)
{
    var response = heartbeatResult.Response;
    Console.WriteLine($"License valid: {response.IsValid}");
    Console.WriteLine($"Product: {response.ProductName}");
    Console.WriteLine($"Expires: {response.ExpiryDate}");
}
else
{
    Console.WriteLine($"Heartbeat failed: {heartbeatResult.Error}");
    Console.WriteLine($"Error code: {heartbeatResult.ErrorCode}");
}

// With custom machine fingerprint
var heartbeatResult = await licenseHelper.SendHeartbeatAsync(
    activationId: myActivationId,
    customerEmail: "customer@example.com",
    machineFingerprint: "custom-machine-id"
);
```

### Result Objects

#### ActivateLicenseResult
```csharp
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
```

#### HeartbeatResult
```csharp
public class HeartbeatResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? ErrorCode { get; set; } // CUSTOMER_MISMATCH, MACHINE_MISMATCH, etc.
    public string? ErrorDetails { get; set; }
    public Guid ActivationId { get; set; }
    public string? CustomerEmail { get; set; }
    public string? MachineFingerprint { get; set; }
    public string? SecurityToken { get; set; }
    public HeartbeatResponse? Response { get; set; }
}
```

### Complete Example: Background Service with Helpers

```csharp
public class LicenseManagementService : BackgroundService
{
    private readonly LicenseHelper _licenseHelper;
    private readonly IConfiguration _configuration;
    private Guid? _activationId;

    public LicenseManagementService(
        LicenseHelper licenseHelper,
        IConfiguration configuration)
    {
        _licenseHelper = licenseHelper;
        _configuration = configuration;
    }

    public async Task<bool> ActivateLicenseAsync(string token)
    {
        var result = await _licenseHelper.ActivateLicenseAsync(
            licenseToken: token,
            productCode: _configuration["License:ProductCode"]
        );

        if (result.Success)
        {
            _activationId = result.ActivationId;
            // Store activation ID for heartbeats
            await SaveActivationIdAsync(result.ActivationId);
            return true;
        }

        return false;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_activationId.HasValue)
            {
                var heartbeat = await _licenseHelper.SendHeartbeatAsync(
                    activationId: _activationId.Value,
                    customerEmail: _configuration["License:CustomerEmail"]
                );

                if (!heartbeat.Success)
                {
                    // Handle license invalidation
                    await HandleLicenseInvalidationAsync(heartbeat.ErrorCode);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

### Console Application Example (Standalone)

```csharp
using Nesco.Licensing.Helpers;
using Nesco.Licensing.Core.Models;

class Program
{
    static async Task Main(string[] args)
    {
        // Create standalone helper
        var licenseHelper = LicenseHelper.CreateStandalone(
            apiBaseUrl: "https://license-api.example.com",
            publicKey: "YOUR_RSA_PUBLIC_KEY"
        );

        // Activate license
        Console.WriteLine("Enter your license token:");
        var token = Console.ReadLine();
        
        var result = await licenseHelper.ActivateLicenseAsync(
            licenseToken: token,
            productCode: "MYPRODUCT"
        );

        if (result.RequiresEula)
        {
            Console.WriteLine($"EULA: {result.RequiredEula.Name}");
            Console.WriteLine(result.RequiredEula.Content);
            Console.WriteLine("\nDo you accept? (y/n)");
            
            if (Console.ReadLine()?.ToLower() == "y")
            {
                var eulaAcceptance = new EulaAcceptanceInfo
                {
                    EulaId = result.RequiredEula.Id,
                    AcceptedByName = "User Name",
                    AcceptedByEmail = "user@example.com",
                    IsAccepted = true
                };
                
                result = await licenseHelper.ActivateLicenseAsync(
                    licenseToken: token,
                    productCode: "MYPRODUCT",
                    eulaAcceptance: eulaAcceptance
                );
            }
        }

        if (result.Success)
        {
            Console.WriteLine($"✓ License activated! ID: {result.ActivationId}");
            
            // Periodic heartbeat
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(5));
                
                var heartbeat = await licenseHelper.SendHeartbeatAsync(
                    activationId: result.ActivationId.Value,
                    customerEmail: "user@example.com"
                );
                
                if (!heartbeat.Success)
                {
                    Console.WriteLine($"License check failed: {heartbeat.Error}");
                    break;
                }
            }
        }
        else
        {
            Console.WriteLine($"✗ Activation failed: {result.Error}");
        }
    }
}
```

## Service Methods (ILicenseActivationService)

The service is automatically registered when you call `AddLicenseActivationComponent()`. Available methods:

```csharp
public interface ILicenseActivationService
{
    // Check for duplicate activations
    Task<DuplicateCheckResponse?> CheckDuplicateActivationAsync(LicenseActivationRequest request);
    
    // Deactivate a license
    Task<bool> DeactivateLicenseAsync(LicenseDeactivationRequest request);
    
    // Validate license status
    Task<LicenseValidationResponse?> ValidateLicenseAsync(LicenseValidationRequest request);
    
    // Send secure heartbeat to check activation validity with token-based authentication
    // Automatically generates machine fingerprint when MachineFingerprint is empty
    Task<HeartbeatResponse?> SendHeartbeatAsync(HeartbeatRequest request);
    
    // Activate with token (handles EULA requirements)
    Task<TokenActivationResponse> ActivateWithTokenAsync(TokenActivationRequest request);
}
```

### HttpLicenseActivationService Implementation

- Configured with base URL from settings
- Endpoints:
  - `POST /api/license/check-duplicate`
  - `POST /api/license/deactivate`
  - `POST /api/license/validate`
  - `POST /api/license/heartbeat`
  - `POST /api/license/activate`

## Key Features

### Two-Step Activation Process
1. First attempts activation without EULA
2. If EULA is required, automatically opens dialog for acceptance
3. After EULA acceptance, completes activation

### EULA Acceptance
- Modal dialog presentation
- Pre-fillable name/email fields via parameters
- Fields hidden when pre-filled values provided
- Validation before activation

### Machine Fingerprinting
Creates unique machine identifiers that are consistent across different browsers on the same machine using:
- Hardware characteristics (CPU cores, memory, platform)
- Physical display properties (resolution, color depth)
- System settings (timezone, installed fonts)
- GPU hardware capabilities (WebGL renderer, vendor)
- Audio hardware specifications
- All hashed with SHA-256 for privacy and uniqueness

#### Machine Fingerprint Parameter
Both activation components support an optional `MachineFingerprint` parameter that allows you to override the fingerprint behavior:

```razor
<!-- Auto-generated fingerprint (default behavior) -->
<MudLicenseActivationComponent ProductCode="MYPRODUCT" />

<!-- Custom fingerprint via parameter -->
<MudLicenseActivationComponent 
    ProductCode="MYPRODUCT" 
    MachineFingerprint="custom-machine-identifier" />

<!-- Using variable from code-behind -->
<HtmlLicenseActivationComponent 
    ProductCode="MYPRODUCT" 
    MachineFingerprint="@storedMachineId" />
```

**Behavior:**
- **When `MachineFingerprint` is provided**: 
  - Uses the provided value as the machine fingerprint
  - Disables the manual Machine ID input field
  - Skips auto-generation completely
  - Field becomes read-only with visual indication

- **When `MachineFingerprint` is NOT provided**:
  - User can manually enter a Machine ID (takes priority)
  - If Manual Machine ID is empty, auto-generates fingerprint
  - Field remains editable and functional

**Use Cases:**
- **Fixed Machine IDs**: When you want to enforce a specific machine identifier
- **Integration with External Systems**: Use fingerprints from other parts of your application
- **Testing**: Provide consistent test machine identifiers
- **Custom Logic**: Generate fingerprints using your own algorithm and pass them to the component

### Secure Heartbeat System
The heartbeat system uses JWT-like tokens for enhanced security validation:

#### Usage Example:
```csharp
// Inject the service
@inject ILicenseActivationService LicenseService

// Create heartbeat request with security token
var heartbeatRequest = new HeartbeatRequest(
    activationId: myActivationId,
    customerEmail: "user@company.com",
    machineFingerprint: "browser-fingerprint-hash"
);

// Send heartbeat (fingerprint auto-generated if empty)
var response = await LicenseService.SendHeartbeatAsync(heartbeatRequest);

if (response?.IsValid == true)
{
    // License is still valid
    Console.WriteLine($"License valid until: {response.ExpiryDate}");
}
else
{
    // License invalid or expired
    Console.WriteLine("License validation failed");
}
```

#### Periodic Heartbeat Implementation:
```csharp
public class LicenseHeartbeatService : IHostedService, IDisposable
{
    private readonly ILicenseActivationService _licenseService;
    private Timer? _timer;
    private readonly Guid _activationId;
    private readonly string _customerEmail;
    private readonly string _machineFingerprint;

    public LicenseHeartbeatService(ILicenseActivationService licenseService)
    {
        _licenseService = licenseService;
        // Initialize with your application's values
        _activationId = GetStoredActivationId();
        _customerEmail = GetCurrentUserEmail();
        _machineFingerprint = GetBrowserFingerprint();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Send heartbeat every 5 minutes
        _timer = new Timer(SendHeartbeat, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        return Task.CompletedTask;
    }

    private async void SendHeartbeat(object? state)
    {
        try
        {
            var request = new HeartbeatRequest(_activationId, _customerEmail, _machineFingerprint);
            // Machine fingerprint is auto-generated if empty for consistent identification
            var response = await _licenseService.SendHeartbeatAsync(request);
            
            if (response?.IsValid != true)
            {
                // Handle license validation failure
                await HandleLicenseInvalidation();
            }
        }
        catch (Exception ex)
        {
            // Log heartbeat failure
            Console.WriteLine($"Heartbeat failed: {ex.Message}");
        }
    }

    private async Task HandleLicenseInvalidation()
    {
        // Implement your license invalidation logic
        // E.g., disable features, show reactivation dialog, etc.
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}
```

#### Security Features:
- **Token-based Authentication**: Uses JWT-like tokens containing `{activationId}.{customerEmail}.{machineFingerprint}`
- **Customer Validation**: Optionally validates that the heartbeat customer matches the license customer
- **Machine Validation**: Optionally validates that the heartbeat machine fingerprint matches the activation fingerprint
- **Configurable Security**: License-level flags `CheckCustomer` and `CheckFingerprint` control validation behavior
- **Replay Attack Protection**: Tokens include unique machine fingerprints and customer identifiers
- **Graceful Degradation**: Empty customer emails or fingerprints are handled gracefully for testing scenarios

#### HeartbeatRequest Structure:
```csharp
public class HeartbeatRequest
{
    public string Token { get; set; } // JWT-like security token
    
    // Read-only properties for accessing token components:
    public string ActivationId { get; }       // Parsed from token
    public string CustomerEmail { get; }      // Parsed from token  
    public string MachineFingerprint { get; } // Parsed from token
    
    // Constructor options:
    public HeartbeatRequest(string token);
    public HeartbeatRequest(Guid activationId, string customerEmail, string machineFingerprint);
}
```

#### HeartbeatResponse Structure:
```csharp
public class HeartbeatResponse
{
    public bool IsValid { get; set; }              // Overall validity
    public bool IsActiveActivation { get; set; }   // Activation status
    public bool IsValidLicense { get; set; }       // License status
    public Guid ActivationId { get; set; }
    public string ProductName { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Params { get; set; }             // Product parameters
}
```

#### Error Handling:
The heartbeat system provides specific error messages for different validation failures:

- **"Invalid token format"** - Token parsing failed
- **"Activation not found"** - ActivationId doesn't exist in database
- **"License not found for this activation"** - License associated with activation is missing
- **"The license does not belong to this customer"** - Customer email validation failed (when `CheckCustomer = true`)
- **"The license does not belong to this machine"** - Machine fingerprint validation failed (when `CheckFingerprint = true`)

#### Auto-Generated Fingerprints:
The `SendHeartbeatAsync` method automatically generates machine fingerprints when empty:

```csharp
// Create request with empty fingerprint - will be auto-generated
var request = new HeartbeatRequest(activationId, customerEmail, "");
var response = await LicenseService.SendHeartbeatAsync(request);

// Create request with specific fingerprint - will use provided value
var request = new HeartbeatRequest(activationId, customerEmail, "specific-fingerprint");  
var response = await LicenseService.SendHeartbeatAsync(request);
```

**Auto-Generation Behavior:**
- When `MachineFingerprint` is **empty string**: Automatically generates machine fingerprint
- When `MachineFingerprint` has **value**: Uses the provided fingerprint
- **Cross-Browser Consistency**: Generated fingerprints are the same across Chrome, Firefox, Edge, etc.
- **Hardware-Based**: Uses actual machine characteristics, not browser-specific features

#### Best Practices:

1. **Store Activation Data Securely**: Keep activation ID, customer email, and machine fingerprint in secure storage
2. **Use Auto-Generated Fingerprints**: Pass empty string for machine fingerprint to enable automatic generation for consistent identification across browsers
3. **Handle Network Failures**: Implement retry logic with exponential backoff for network errors
4. **Graceful Degradation**: Continue operation when heartbeat fails, but limit functionality after extended failures
5. **Frequency Balance**: Too frequent heartbeats waste bandwidth; too infrequent allows longer unauthorized usage
6. **Security Configuration**: Enable `CheckCustomer` and `CheckFingerprint` for production licenses
7. **Monitor Heartbeat Health**: Track successful/failed heartbeats for monitoring and analytics

### Shared Utilities
The `ActivationUtilities` class provides:
- `ValidateLicenseToken()` - Token validation with product code and signature checks
- `ValidateEulaAcceptance()` - EULA field validation
- `GetFinalEulaValues()` - Priority handling for parameter vs form values
- `IsEulaRequired()` - Checks if response requires EULA

## Requirements

- .NET 9.0 or later
- Blazor Server, WebAssembly, or Hybrid
- MudBlazor 8.0+ (only for MudLicenseActivationComponent)

## License

MIT License

---

Built with Blazor and MudBlazor