using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_FerienArbeitspensum")]
[Index("MitarbeiterId", "GueltigAb", Name = "UK_FerienArbeitspensum", IsUnique = true)]
public partial class TFerienArbeitspensum
{
    [Key]
    public Guid Id { get; set; }

    public Guid MitarbeiterId { get; set; }

    [Column(TypeName = "numeric(3, 0)")]
    public decimal FerienProJahr { get; set; }

    [Column(TypeName = "numeric(3, 0)")]
    public decimal Arbeitspensum { get; set; }

    public DateOnly GueltigAb { get; set; }

    public bool Montag { get; set; }

    public bool Dienstag { get; set; }

    public bool Mittwoch { get; set; }

    public bool Donnerstag { get; set; }

    public bool Freitag { get; set; }

    [ForeignKey("MitarbeiterId")]
    [InverseProperty("TFerienArbeitspensum")]
    public virtual TMitarbeiter Mitarbeiter { get; set; } = null!;
}
