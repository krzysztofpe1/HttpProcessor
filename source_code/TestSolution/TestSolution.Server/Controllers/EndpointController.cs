using Microsoft.AspNetCore.Mvc;

namespace TestSolution.Server.Controllers;

/// <summary>
/// Endpoint controller
/// </summary>
[Route("/api/[controller]")]
[ApiController]
public class EndpointController : Controller
{

    /// <summary>
    /// Get
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }

}
