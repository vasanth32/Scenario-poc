using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace EntraProductionPoc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    [HttpGet]
    [RequiredScope("access_as_user")]
    public IActionResult GetOrders()
    {
        return Ok(new
        {
            message = "You successfully accessed the Orders API.",
            user = User.Identity?.Name,
            authenticated = User.Identity?.IsAuthenticated
        });
    }
}