namespace Application.DTOs;

public class ReceivedSmsDto
{
    public string SenderName { get; set; } = string.Empty;

    public string SmsContent { get; set; } = string.Empty;

    public DateTime ReceivedAt { get; set; }
}
