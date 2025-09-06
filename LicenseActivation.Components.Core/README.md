# LicenseActivation.Components.Core

[![NuGet Version](https://img.shields.io/nuget/v/Nesco.LicenseActivation.blazor.Core)](https://www.nuget.org/packages/Nesco.LicenseActivation.blazor.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com)

Core library for the LicenseActivation.Components Blazor component library. Contains shared models, interfaces, and services used for license activation functionality.

## Features

- **Shared Models** - Common data models for license activation
- **API Models** - Request/response DTOs for license activation APIs
- **Token Services** - Client token generation and validation
- **Component Models** - Shared models for UI components

## Installation

```bash
dotnet add package Nesco.LicenseActivation.blazor.Core
```

## Requirements

- **.NET 9.0** or later

## Models

### License Models
- `LicenseFile` - License file data structure
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

### Component Models
- `ComponentType` - Enum for component types (Mud, Html, Token)
- `MessageType` - Enum for message types (Info, Success, Warning, Error)

## Services

### IClientTokenService / ClientTokenService
- Generate unique client tokens
- Validate token formats
- Extract token information

## Usage

This library is typically used as a dependency of `Nesco.LicenseActivation.Blazor` and provides the core functionality and models for license activation components.

## License

MIT License - see LICENSE file for details
