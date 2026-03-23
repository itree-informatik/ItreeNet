namespace ItreeNet.Data.Models
{
    public class PipelineRunResponse
    {
        public int Count { get; set; }
        public List<PipelineRun>? Value { get; set; }
    }
}
