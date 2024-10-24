using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API2.Controllers
{
  // api2 ana servis ismi

  [Route("api2/[controller]")]
  [ApiController]
  public class TestsController : ControllerBase
  {

    [HttpGet]
    public async Task<IActionResult> Get()
    {
      return Ok("API2");
    }
  }
}
