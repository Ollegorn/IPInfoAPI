using IPInfoAPI.Models;
using IPInfoLibrary;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace IPInfoAPI.Services;

public class BatchProcessingService : BackgroundService
{
    private readonly IIPInfoProvider _ipInfoProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<Guid, BatchJobStatus> _jobStatuses;
    private readonly ConcurrentQueue<(Guid JobId, List<IPDetailsUpdateDto> Updates)> _queue;

    public BatchProcessingService(
        IIPInfoProvider ipInfoProvider,
        IServiceScopeFactory scopeFactory)
    {
        _ipInfoProvider = ipInfoProvider;
        _scopeFactory = scopeFactory;
        _jobStatuses = new ConcurrentDictionary<Guid, BatchJobStatus>();
        _queue = new ConcurrentQueue<(Guid, List<IPDetailsUpdateDto>)>();
    }

    public Guid QueueBatchJob(List<IPDetailsUpdateDto> updates)
    {
        var jobId = Guid.NewGuid();
        var status = new BatchJobStatus
        {
            JobId = jobId,
            TotalItems = updates.Count,
            ProcessedItems = 0,
            Status = "Queued"
        };

        _jobStatuses.TryAdd(jobId, status);
        _queue.Enqueue((jobId, updates));
        return jobId;
    }

    public BatchJobStatus GetJobStatus(Guid jobId)
    {
        return _jobStatuses.TryGetValue(jobId, out var status) ? status : null;
    }
    public IEnumerable<BatchJobStatus> GetAllJobs()
    {
        return _jobStatuses.Values.ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var job))
            {
                await ProcessBatchJobAsync(job.JobId, job.Updates, cancellationToken);
            }
            else
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    private async Task ProcessBatchJobAsync(Guid jobId, List<IPDetailsUpdateDto> updates, CancellationToken cancellationToken)
    {
        try
        {
            if (_jobStatuses.TryGetValue(jobId, out var status))
            {
                status.Status = "Processing";

                for (int i = 0; i < updates.Count; i += 10)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var batch = updates.Skip(i).Take(10).ToList();
                    await ProcessBatchAsync(batch, status);

                    status.ProcessedItems += batch.Count;
                }

                status.Status = "Completed";
            }
        }
        catch (Exception ex)
        {
            if (_jobStatuses.TryGetValue(jobId, out var status))
            {
                status.Status = "Failed";
                status.Errors.Add($"Job failed: {ex.Message}");
            }
        }
    }

    private async Task ProcessBatchAsync(List<IPDetailsUpdateDto> updates, BatchJobStatus status)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IPInfoDbContext>();

        using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            foreach (var update in updates)
            {
                var existingIP = await dbContext.IPDetails
                    .FirstOrDefaultAsync(x => x.IP == update.IP);

                if (existingIP == null)
                {
                    status.Errors.Add($"IP {update.IP} not found");
                    continue;
                }

                existingIP.City = update.City;
                existingIP.Country = update.Country;
                existingIP.Continent = update.Continent;
                existingIP.Latitude = update.Latitude;
                existingIP.Longitude = update.Longitude;
            }

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            status.Errors.Add($"Error: {ex.Message}");
            throw;
        }
        await Task.Delay(10000); // fake proccessing time
    }
}