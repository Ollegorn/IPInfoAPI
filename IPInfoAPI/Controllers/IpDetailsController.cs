using IPInfoLibrary;
using Microsoft.AspNetCore.Mvc;

namespace IPInfoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IpDetailsController : ControllerBase
    {
        private readonly IIPInfoProvider _ipInfoProvider;

        public IpDetailsController(IIPInfoProvider ipInfoProvider)
        {
            _ipInfoProvider = ipInfoProvider;
        }

        [HttpGet("{ip}")]
        public async Task<IActionResult> GetIPDetails(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return BadRequest("IP address cannot be null or empty.");

            var details = await _ipInfoProvider.GetDetails(ip);
            return Ok(details);
        }
    }
}
