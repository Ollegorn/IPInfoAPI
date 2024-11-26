using Microsoft.AspNetCore.Mvc;
using IPInfoLibrary;
using IPInfoAPI.Repositories;
using IPInfoAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IPInfoAPI.Controllers;

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
    public async Task<Results<Ok<IPDetailsEntity>,NotFound, BadRequest>> GetIPDetails(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return TypedResults.NotFound();

        try
        {
            var existingDetails = await _ipDetailsRepository.GetIPDetailsAsync(ip);
            if (existingDetails != null)
            {
                return TypedResults.Ok(existingDetails);
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

            return TypedResults.Ok(ipDetails);
        }
        catch (IPServiceNotAvailableException ex)
        {
            return TypedResults.BadRequest();
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest();
        }
    }
}
