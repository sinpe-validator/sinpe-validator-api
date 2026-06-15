namespace Application.DTOs;

public class UnmatchedPaymentDto
{
    public int IdOrderPayment { get; set; }

    public int IdSms { get; set; }
    public string? SenderName { get; set; }
    public decimal? Amount { get; set; }
    public string? SinpeReference { get; set; }
    public string? SmsDescription { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? RegisteredAt { get; set; }

    public int PaymentStatusId { get; set; }
    public string? PaymentStatusName { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime ProcessedAt { get; set; }
}
