using Microsoft.AspNetCore.Mvc;
using IPInfoLibrary;
using IPInfoAPI.Repositories;
using IPInfoAPI.Data;

namespace IPInfoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IpDetailsController : ControllerBase
    {
        private readonly IIPDetailsRepository _ipDetailsRepository;
        private readonly IIPInfoProvider _ipInfoProvider;

        public IpDetailsController(IIPDetailsRepository ipDetailsRepository, IIPInfoProvider ipInfoProvider)
        {
            _ipDetailsRepository = ipDetailsRepository;
            _ipInfoProvider = ipInfoProvider;
        }

        [HttpGet("{ip}")]
        public async Task<IActionResult> GetIPDetails(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return BadRequest("IP address cannot be null or empty.");

            try
            {
                var existingDetails = await _ipDetailsRepository.GetIPDetailsAsync(ip);
                if (existingDetails != null)
                {
                    return Ok(existingDetails);
                }

                var detailsFromLibrary = await _ipInfoProvider.GetDetails(ip);

                var ipDetails = new IPDetailsEntity
                {
                    IP = ip,
                    City = detailsFromLibrary.City,
                    Country = detailsFromLibrary.Country,
                    Continent = detailsFromLibrary.Continent,
                    Latitude = detailsFromLibrary.Latitude,
                    Longitude = detailsFromLibrary.Longitude
                };

                await _ipDetailsRepository.AddIPDetailsAsync(ipDetails);

                return Ok(ipDetails);
            }
            catch (IPServiceNotAvailableException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
