using Consul;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly;

namespace API1.Controllers
{
  // api1 ana servis ismi

  [Route("api1/[controller]")]
  [ApiController]
  public class TestsController : ControllerBase
  {
    private readonly IConsulClient consulClient;

    public TestsController(IConsulClient consulClient)
    {
      this.consulClient = consulClient;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
      
      // Recilency

      await Polly.Policy.Handle<Exception>().RetryAsync(3, (exception, retryCount) =>
       {
         Console.Out.WriteLineAsync("Hata Mesajı :" + exception.Message);
         Console.Out.WriteLineAsync("Retry Count:" + retryCount);

       }).ExecuteAsync(async () =>
       {
         // Hata için api3 ile dene
         var service = (await consulClient.Agent.GetServiceConfiguration("api2")).Response;

         ArgumentNullException.ThrowIfNull(service);

         var url = $"http://{service.Address}:{service.Port}/{service.Service}/tests";

         await Console.Out.WriteLineAsync(url);

         using var httpClient = new HttpClient();
         var response = await httpClient.GetAsync(url);

         if (response.IsSuccessStatusCode)
         {
           string data = await response.Content.ReadAsStringAsync();
           await Console.Out.WriteLineAsync("Response =>" + data);
         }
       });

      return Ok("API1");
    }
  }
}
