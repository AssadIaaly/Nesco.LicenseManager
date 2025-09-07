namespace Nesco.Licensing.Core.Models;

/// <summary>
/// Token structure for secure heartbeat requests
/// Format: {activationId}.{customerEmail}.{machineFingerprint}
/// </summary>
public class HeartbeatToken
{
    public Guid ActivationId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string MachineFingerprint { get; set; } = string.Empty;

    public HeartbeatToken() { }

    public HeartbeatToken(Guid activationId, string customerEmail, string machineFingerprint)
    {
        ActivationId = activationId;
        CustomerEmail = customerEmail ?? string.Empty;
        MachineFingerprint = machineFingerprint ?? string.Empty;
    }

    /// <summary>
    /// Create JWT-like token string: {activationId}.{customerEmail}.{machineFingerprint}
    /// </summary>
    public string ToToken()
    {
        var activationIdStr = Convert.ToBase64String(ActivationId.ToByteArray()).TrimEnd('=');
        
        // Handle null/empty values by using placeholder
        var safeEmail = string.IsNullOrEmpty(CustomerEmail) ? "EMPTY" : CustomerEmail;
        var safeFingerprint = string.IsNullOrEmpty(MachineFingerprint) ? "EMPTY" : MachineFingerprint;
        
        var emailStr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(safeEmail)).TrimEnd('=');
        var fingerprintStr = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(safeFingerprint)).TrimEnd('=');
        
        return $"{activationIdStr}.{emailStr}.{fingerprintStr}";
    }

    /// <summary>
    /// Parse JWT-like token string back to HeartbeatToken
    /// </summary>
    public static HeartbeatToken? FromToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            // Pad base64 strings if needed
            var activationIdStr = PadBase64(parts[0]);
            var emailStr = PadBase64(parts[1]);
            var fingerprintStr = PadBase64(parts[2]);

            var activationIdBytes = Convert.FromBase64String(activationIdStr);
            var activationId = new Guid(activationIdBytes);
            
            var customerEmail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(emailStr));
            var machineFingerprint = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(fingerprintStr));

            // Convert placeholder back to empty string
            customerEmail = customerEmail == "EMPTY" ? string.Empty : customerEmail;
            machineFingerprint = machineFingerprint == "EMPTY" ? string.Empty : machineFingerprint;

            return new HeartbeatToken(activationId, customerEmail, machineFingerprint);
        }
        catch
        {
            return null;
        }
    }

    private static string PadBase64(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
            
        var remainder = input.Length % 4;
        if (remainder > 0)
        {
            var padding = 4 - remainder;
            input += new string('=', padding);
        }
        return input;
    }
}