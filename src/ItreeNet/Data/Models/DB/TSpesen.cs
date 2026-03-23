using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_Spesen")]
public partial class TSpesen
{
    [Key]
    public Guid Id { get; set; }

    public Guid MitarbeiterId { get; set; }

    public DateOnly Datum { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal Betrag { get; set; }

    [StringLength(250)]
    public string AnlassOrt { get; set; } = null!;

    [StringLength(250)]
    public string Spesenart { get; set; } = null!;

    public bool Kreditkarte { get; set; }

    public DateOnly? EingereichtAm { get; set; }

    [ForeignKey("MitarbeiterId")]
    [InverseProperty("TSpesen")]
    public virtual TMitarbeiter Mitarbeiter { get; set; } = null!;
}
