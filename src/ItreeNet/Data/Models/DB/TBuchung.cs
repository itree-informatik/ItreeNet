using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_Buchung")]
public partial class TBuchung
{
    [Key]
    public Guid Id { get; set; }

    public Guid VorgangId { get; set; }

    public Guid MitarbeiterId { get; set; }

    public DateOnly Datum { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ZeitVon { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ZeitBis { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Zeit { get; set; }

    [StringLength(100)]
    public string Buchungstext { get; set; } = null!;

    public bool Stunden { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Abgerechnet { get; set; }

    public bool Provisorisch { get; set; }

    public Guid? ChangedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ChangedOn { get; set; }

    public Guid? OriginalVorgangId { get; set; }

    public DateOnly? OriginalDatum { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? OriginalZeit { get; set; }

    [StringLength(100)]
    public string? OriginalText { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("TBuchungChangedByNavigation")]
    public virtual TMitarbeiter? ChangedByNavigation { get; set; }

    [ForeignKey("MitarbeiterId")]
    [InverseProperty("TBuchungMitarbeiter")]
    public virtual TMitarbeiter Mitarbeiter { get; set; } = null!;

    [ForeignKey("OriginalVorgangId")]
    [InverseProperty("TBuchungOriginalVorgang")]
    public virtual TVorgang? OriginalVorgang { get; set; }

    [ForeignKey("VorgangId")]
    [InverseProperty("TBuchungVorgang")]
    public virtual TVorgang Vorgang { get; set; } = null!;
}
