namespace LicenseActivation.Components.Core.Models;

/// <summary>
/// Secure heartbeat request with token-based authentication
/// </summary>
public class HeartbeatRequest
{
    /// <summary>
    /// JWT-like security token containing: {activationId}.{customerEmail}.{machineFingerprint}
    /// </summary>
    public string Token { get; set; } = string.Empty;

    public HeartbeatRequest() { }

    public HeartbeatRequest(string token)
    {
        Token = token;
    }

    public HeartbeatRequest(Guid activationId, string customerEmail, string machineFingerprint)
    {
        var heartbeatToken = new HeartbeatToken(activationId, customerEmail, machineFingerprint);
        Token = heartbeatToken.ToToken();
    }
    public string ActivationId => HeartbeatToken.FromToken(Token)?.ActivationId.ToString() ?? string.Empty;
    public string CustomerEmail => HeartbeatToken.FromToken(Token)?.CustomerEmail ?? string.Empty;
    public string MachineFingerprint => HeartbeatToken.FromToken(Token)?.MachineFingerprint ?? string.Empty;
}