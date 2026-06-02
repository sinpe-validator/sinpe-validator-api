using System;
using System.Collections.Generic;
using System.Text;
using Application.Contracts;

namespace Application.Common.Results;

public class OrderServiceResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public int? OrderId { get; set; }
    public string? OrderCode { get; set; }
}
