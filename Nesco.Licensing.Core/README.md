# LicenseActivation.Components.Core

[![NuGet Version](https://img.shields.io/nuget/v/Nesco.Licensing.Core)](https://www.nuget.org/packages/Nesco.Licensing.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com)

Core library for the Nesco.Licensing Blazor component library. Contains shared models, interfaces, and services used for license activation functionality.

## Features

- **Shared Models** - Common data models for license activation
- **API Models** - Request/response DTOs for license activation APIs
- **Token Services** - Client token generation and validation
- **Component Models** - Shared models for UI components

## Installation

```bash
dotnet add package Nesco.Licensing.Core
```

## Requirements

- **.NET 9.0** or later

## Models

### License Models
- `LicenseActivationResult` - Activation response data
- `LicenseActivationMessage` - UI message models
- `LicenseActivationSettings` - Component configuration

### API Models  
- `LicenseActivationRequest` - Activation request DTO
- `DuplicateCheckRequest` - Duplicate detection request
- `DuplicateCheckResponse` - Duplicate detection response
- `HeartbeatResponse` - License heartbeat response

### Token Models
- `TokenActivationRequest` - Token-based activation request
- `TokenActivationResult` - Token activation response
- `TokenValidationResult` - Token validation response
- `ApiError` - API error response model

## Usage

This library is typically used as a dependency of `Nesco.Licensing` and provides the core functionality and models for license activation components.

## License

MIT License - see LICENSE file for details
