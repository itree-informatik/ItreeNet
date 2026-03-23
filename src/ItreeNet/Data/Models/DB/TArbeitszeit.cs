using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_Arbeitszeit")]
[Index("Jahr", "Monat", Name = "UK_Arbeitszeit", IsUnique = true)]
public partial class TArbeitszeit
{
    [Key]
    public Guid Id { get; set; }

    public int Jahr { get; set; }

    public int Monat { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal Arbeitszeit { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal Tagesarbeitszeit { get; set; }
}
