using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_ArbeitszeitReduktion")]
[Index("Datum", Name = "UK_ArbeitszeitReduktion", IsUnique = true)]
public partial class TArbeitszeitReduktion
{
    [Key]
    public Guid Id { get; set; }

    public DateOnly Datum { get; set; }

    [Column(TypeName = "numeric(3, 2)")]
    public decimal Reduktion { get; set; }
}
