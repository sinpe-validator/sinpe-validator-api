namespace Application.DTOs;

public class ReceivedSmsDto
{
    public string SenderName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string SinpeReference { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ReceivedAt { get; set; }
}
