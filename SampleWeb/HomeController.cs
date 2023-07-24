using Microsoft.AspNetCore.Mvc;

public class Result<T>
{
    public string Name { get; set; }
    public T Value { get; set; }

    public Result(string name, T value)
    {
        Name = name;
        Value = value;
    }
}

public class HomeController : Controller
{
    [HttpGet]
    public ActionResult<Result<bool>> IsOdd([FromQuery] int value, [FromQuery] string? name = null)
    {
        return Ok(new Result<bool>(name ?? "", value % 2 == 1));
    }
}