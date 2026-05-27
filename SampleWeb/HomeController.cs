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

// RESTful attribute-routed sample, exercising the 10.1.0 features:
// - controller + action route templates combined
// - Guid mapped to string
// - implicit route binding via {id} matching the parameter name
// - CancellationToken filtered out
// - route constraints (:guid) stripped from the interpolated URL
[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{
    public record OrderDto(Guid Id, string Reference);
    public record CreateOrderRequest(string Reference);

    [HttpGet]
    public ActionResult<OrderDto[]> List(CancellationToken ct) => Ok(Array.Empty<OrderDto>());

    [HttpGet("{id:guid}")]
    public ActionResult<OrderDto> Get(Guid id, CancellationToken ct) => Ok(new OrderDto(id, "REF-1"));

    [HttpPost]
    public ActionResult<OrderDto> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        => Ok(new OrderDto(Guid.NewGuid(), request.Reference));

    [HttpPut("{id:guid}/assign")]
    public ActionResult<bool> Assign(Guid id, [FromBody] CreateOrderRequest request, CancellationToken ct) => Ok(true);
}