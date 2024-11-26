using Microsoft.EntityFrameworkCore;
using IPInfoAPI.Models;

namespace IPInfoAPI.Repositories;

public class IPDetailsRepository : IIPDetailsRepository
{
    private readonly IPInfoDbContext _dbContext;

    public IPDetailsRepository(IPInfoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IPDetailsEntity> GetIPDetailsAsync(string ip)
    {
        return await _dbContext.IPDetails.FirstOrDefaultAsync(x => x.IP == ip);
    }

    public async Task AddIPDetailsAsync(IPDetailsEntity ipDetails)
    {
        _dbContext.IPDetails.Add(ipDetails);
        await _dbContext.SaveChangesAsync();
    }
}
