﻿using IPInfoAPI.Data;

namespace IPInfoAPI.Repositories
{
    public interface IIPDetailsRepository
    {
        Task<IPDetailsEntity> GetIPDetailsAsync(string ip);
        Task AddIPDetailsAsync(IPDetailsEntity ipDetails);
    }
}
