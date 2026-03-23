namespace ItreeNet.Data.Models
{
    public class PipelineRun
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? FinishedDate { get; set; }
        public string? Url { get; set; }
        public string? Result { get; set; }
        public string? State { get; set; }
        public string? Name { get; set; }
        public int Revision { get; set; }
        public PipelineReference? Pipeline { get; set; }
        public Links? _links { get; set; }
    }
}
