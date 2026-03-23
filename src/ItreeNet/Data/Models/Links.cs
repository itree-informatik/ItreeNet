namespace ItreeNet.Data.Models
{
    public class Links
    {
        public Link? Self { get; set; }
        public Link? Web { get; set; }
        public Link? PipelineWeb { get; set; }
        public Link? Pipeline { get; set; }
    }

    public class Link
    {
        public string? Href { get; set; }
    }
}
