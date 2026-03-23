namespace ItreeNet.Data.Models;

public partial class PipelineRuns
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = null!;
    public string PipelineName { get; set; } = null!;
    public string PipelineId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Result { get; set; } = null!;
    public string Link { get; set; } = null!;
    public DateTime LastExecution { get; set; }
}
