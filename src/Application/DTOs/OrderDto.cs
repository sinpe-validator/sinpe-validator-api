namespace Application.DTOs;

public class OrderDto
{
    public int IdOrder { get; set; }

    public string OrderCode { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public string? Description { get; set; }
}
