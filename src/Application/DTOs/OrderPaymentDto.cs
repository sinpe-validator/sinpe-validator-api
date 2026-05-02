namespace Application.DTOs;

public class OrderPaymentDto
{
    public int IdOrderPayment { get; set; }
    public int? IdOrder { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime ProcessedAt { get; set; }
}
