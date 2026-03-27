using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_Release")]
public partial class TRelease
{
    [Key]
    public Guid Id { get; set; }

    public Guid ApplikationId { get; set; }

    public DateOnly Datum { get; set; }

    [Required]
    [StringLength(255)]
    public string Bezeichnung { get; set; } = null!;

    [ForeignKey("ApplikationId")]
    [InverseProperty("TRelease")]
    public virtual TApplikation Applikation { get; set; } = null!;
}
