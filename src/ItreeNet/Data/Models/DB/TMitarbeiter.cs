using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_Mitarbeiter")]
public partial class TMitarbeiter
{
    [Key]
    public Guid Id { get; set; }

    public Guid? AzureId { get; set; }

    [StringLength(100)]
    public string Nachname { get; set; } = null!;

    [StringLength(100)]
    public string Vorname { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    public DateOnly Eintritt { get; set; }

    public DateOnly? Austritt { get; set; }

    public bool Intern { get; set; }

    public bool Aktiv { get; set; }

    [InverseProperty("ChangedByNavigation")]
    public virtual ICollection<TBuchung> TBuchungChangedByNavigation { get; set; } = new List<TBuchung>();

    [InverseProperty("Mitarbeiter")]
    public virtual ICollection<TBuchung> TBuchungMitarbeiter { get; set; } = new List<TBuchung>();

    [InverseProperty("Mitarbeiter")]
    public virtual ICollection<TFerienArbeitspensum> TFerienArbeitspensum { get; set; } = new List<TFerienArbeitspensum>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<TMitarbeiterSaldoKorrektur> TMitarbeiterSaldoKorrekturCreatedByNavigation { get; set; } = new List<TMitarbeiterSaldoKorrektur>();

    [InverseProperty("Mitarbeiter")]
    public virtual ICollection<TMitarbeiterSaldoKorrektur> TMitarbeiterSaldoKorrekturMitarbeiter { get; set; } = new List<TMitarbeiterSaldoKorrektur>();

    [InverseProperty("Mitarbeiter")]
    public virtual ICollection<TMitarbeiterTeam> TMitarbeiterTeam { get; set; } = new List<TMitarbeiterTeam>();

    [InverseProperty("Mitarbeiter")]
    public virtual ICollection<TProfil> TProfil { get; set; } = new List<TProfil>();

    [InverseProperty("Mitarbeiter")]
    public virtual ICollection<TSpesen> TSpesen { get; set; } = new List<TSpesen>();

    [InverseProperty("Mitarbeiter")]
    public virtual ICollection<TAnwesenheit> TAnwesenheit { get; set; } = new List<TAnwesenheit>();
}
