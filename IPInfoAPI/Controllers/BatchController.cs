using IPInfoAPI.Models;
using IPInfoAPI.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace IPInfoAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BatchController : ControllerBase
{
    private readonly BatchProcessingService _batchProcessingService;

    public BatchController(BatchProcessingService batchProcessingService)
    {
        _batchProcessingService = batchProcessingService;
    }

    [HttpPost("update")]
    public async Task<Results<Ok<Guid>, BadRequest<string>>> SubmitBatchUpdate([FromBody] BatchUpdateRequest request)
    {
        if (request?.Updates == null || !request.Updates.Any())
        {
            return TypedResults.BadRequest("No updates provided");
        }

        foreach (var update in request.Updates)
        {
            if (string.IsNullOrEmpty(update.IP))
            {
                return TypedResults.BadRequest("IP address is required for all updates");
            }
        }

        var jobId = await Task.Run(() => _batchProcessingService.QueueBatchJob(request.Updates));
        return TypedResults.Ok(jobId);
    }

    [HttpGet("status/{jobId}")]
    public async Task<Results<Ok<BatchJobStatus>, NotFound>> GetJobStatus(Guid jobId)
    {
        var status = await Task.Run(() => _batchProcessingService.GetJobStatus(jobId));
        if (status == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(status);
    }

    [HttpGet("jobs")]
    public async Task<Results<Ok<object>, StatusCodeHttpResult>> GetAllJobs()
    {
        try
        {
            var jobs = await Task.FromResult(_batchProcessingService.GetAllJobs());

            var result = new
            {
                TotalCount = jobs.Count(),
                Jobs = jobs
            };

            return TypedResults.Ok((object)result);
        }
        catch (Exception ex)
        {
            return TypedResults.StatusCode(500);
        }
    }


}
