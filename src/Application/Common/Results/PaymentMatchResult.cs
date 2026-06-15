namespace Application.Common.Results;

public enum PaymentOperationError
{
    None = 0,
    NotFound,
    Validation,
    Internal
}

public class PaymentMatchResult
{
    public bool IsSuccess { get; set; }
    public PaymentOperationError Error { get; set; }
    public string? ErrorMessage { get; set; }

    public string? Message { get; set; }
    public int IdOrderPayment { get; set; }
    public int IdOrder { get; set; }
    public string? OrderCode { get; set; }
    public int OrderStatusId { get; set; }
    public int PaymentStatusId { get; set; }
}
