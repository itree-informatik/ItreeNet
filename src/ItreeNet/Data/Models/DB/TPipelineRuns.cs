using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_PipelineRuns")]
public partial class TPipelineRuns
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(100)]
    public string ProjectName { get; set; } = null!;

    [StringLength(255)]
    public string PipelineName { get; set; } = null!;

    [StringLength(50)]
    public string Status { get; set; } = null!;

    [StringLength(50)]
    public string Result { get; set; } = null!;

    [StringLength(255)]
    public string Link { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime LastExecution { get; set; }
}
