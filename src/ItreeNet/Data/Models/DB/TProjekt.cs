using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_Projekt")]
public partial class TProjekt
{
    [Key]
    public Guid Id { get; set; }

    public Guid KundeId { get; set; }

    [StringLength(100)]
    public string Nummer { get; set; } = null!;

    [StringLength(100)]
    public string Bezeichnung { get; set; } = null!;

    public bool Aktiv { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal Gesamtkosten { get; set; }

    public bool Pauschal { get; set; }

    [StringLength(100)]
    public string? Vertragsdauer { get; set; }

    public bool Mehrwertsteuer { get; set; }

    public int? BuchungsintervallMinuten { get; set; }

    public int? Gesamtzeit { get; set; }

    public bool EmailGesendet80 { get; set; }

    public bool EmailGesendet90 { get; set; }

    public bool EmailGesendet100 { get; set; }

    [ForeignKey("KundeId")]
    [InverseProperty("TProjekt")]
    public virtual TKunde Kunde { get; set; } = null!;

    [InverseProperty("Projekt")]
    public virtual ICollection<TVorgang> TVorgang { get; set; } = new List<TVorgang>();
}
