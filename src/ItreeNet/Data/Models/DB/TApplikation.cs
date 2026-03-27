using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_Applikation")]
public partial class TApplikation
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Bezeichnung { get; set; } = null!;

    public virtual ICollection<TRelease> TRelease { get; set; } = new List<TRelease>();
}
