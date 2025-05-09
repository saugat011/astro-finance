using Microsoft.AspNetCore.Mvc;

namespace AstroFinance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "AstroFinance API is running" });
    }
}