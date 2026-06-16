using System;

namespace sinpe_validator_api.Domain.Entities;

public class DeviceHeartbeat
{
    public int IdDevice { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime LastConnection { get; set; }
    public bool IsActive { get; set; } 
}
