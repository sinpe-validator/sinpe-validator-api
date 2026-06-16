namespace Application.Commands
{
    public class ProcessHeartbeatCommand
    {
        public string DeviceId { get; set; }
        public string Timestamp { get; set; }

        public ProcessHeartbeatCommand(string deviceId, string timestamp)
        {
            DeviceId = deviceId;
            Timestamp = timestamp;
        }
    }
}
