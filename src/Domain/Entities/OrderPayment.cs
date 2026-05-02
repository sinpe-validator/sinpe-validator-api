
namespace sinpe_validator_api.Domain.Entities;

public class OrderPayment
{
    public int IdOrderPayment { get; set; }
    public int? IdOrder { get; set; }
    public int IdSms { get; set; }
    public int IdStatus { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime ProcessedAt { get; set; }

    public Order? Order { get; set; }
    public ReceivedSms? Sms { get; set; }
    public PaymentStatus? Status { get; set; }
}
