using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_Anwesenheit")]
public partial class TAnwesenheit
{
    [Key]
    public Guid Id { get; set; }

    public Guid MitarbeiterId { get; set; }

    public DateOnly Datum { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ZeitVon { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ZeitBis { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Zeit { get; set; }

    [StringLength(20)]
    public string Typ { get; set; } = null!;

    [StringLength(500)]
    public string? Notiz { get; set; }

    [ForeignKey("MitarbeiterId")]
    [InverseProperty("TAnwesenheit")]
    public virtual TMitarbeiter Mitarbeiter { get; set; } = null!;
}
