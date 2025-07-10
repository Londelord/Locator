using Microsoft.AspNetCore.Mvc;

namespace Locator.API.Controllers;

[ApiController]
[Route("/")]
public class RootController : ControllerBase
{
    [HttpGet]
    public IActionResult Hello() => Ok("Hello from root");
    
    [HttpGet("health")]
    public IActionResult Health() => Ok("Healthy");
    
}