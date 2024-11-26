namespace IPInfoAPI.Models;

public class BatchJobStatus
{
    public Guid JobId { get; set; }
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public string Status { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
