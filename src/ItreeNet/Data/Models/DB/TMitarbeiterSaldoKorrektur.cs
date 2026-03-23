using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_MitarbeiterSaldoKorrektur")]
public partial class TMitarbeiterSaldoKorrektur
{
    [Key]
    public Guid Id { get; set; }

    public Guid MitarbeiterId { get; set; }

    public int Jahr { get; set; }

    public int Monat { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Stunden { get; set; }

    public int? Ferien { get; set; }

    [StringLength(4000)]
    public string Grund { get; set; } = null!;

    public Guid CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("TMitarbeiterSaldoKorrekturCreatedByNavigation")]
    public virtual TMitarbeiter CreatedByNavigation { get; set; } = null!;

    [ForeignKey("MitarbeiterId")]
    [InverseProperty("TMitarbeiterSaldoKorrekturMitarbeiter")]
    public virtual TMitarbeiter Mitarbeiter { get; set; } = null!;
}
