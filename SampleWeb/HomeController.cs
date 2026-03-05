using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

public class Result<T>
{
    public string?[] Nullable { get; set; }
    // public string Name { get; set; }
    // public T Value { get; set; }
    //
    //
    // public string[] StringArray { get; set; }
    
}

public enum OrderStatus
{
    [Description("Order is pending approval")]
    Pending,
    [Description("Order has been confirmed")]
    Confirmed,
    [Description("Order is being processed")]
    Processing,
    [Description("Order has been shipped")]
    Shipped,
    [Description("Order has been delivered")]
    Delivered,
    [Description("Order was cancelled")]
    Cancelled
}

// Enum without descriptions - should NOT generate description record
public enum Priority
{
    Low,
    Medium,
    High
}

public class HomeController : Controller
{
    [HttpGet]
    public ActionResult<Result<bool>> IsOdd([FromQuery] int value, [FromQuery] string? name = null)
    {
        return Ok(new Result<bool>());
    }

    [HttpGet]
    public ActionResult<OrderStatus> GetOrderStatus([FromQuery] int orderId)
    {
        return Ok(OrderStatus.Pending);
    }

    [HttpGet]
    public ActionResult<Priority> GetPriority([FromQuery] int taskId)
    {
        return Ok(Priority.Medium);
    }
}