using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_MitarbeiterSaldo")]
[Index("MitarbeiterId", "Jahr", "Monat", Name = "UK_MitarbeiterSaldo", IsUnique = true)]
public partial class TMitarbeiterSaldo
{
    [Key]
    public Guid Id { get; set; }

    public Guid MitarbeiterId { get; set; }

    public int Jahr { get; set; }

    public int Monat { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal StundenSaldo { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal FerienSaldo { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal Soll { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal Ist { get; set; }
}
