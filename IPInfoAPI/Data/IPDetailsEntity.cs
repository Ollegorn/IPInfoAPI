using IPInfoLibrary;

namespace IPInfoAPI.Data;
public class IPDetailsEntity : IPDetails
{
    public int Id { get; set; }
    public string IP { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string Continent { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
}

