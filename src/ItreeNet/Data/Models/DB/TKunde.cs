using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_Kunde")]
public partial class TKunde
{
    [Key]
    public Guid Id { get; set; }

    public Guid? TeamId { get; set; }

    [StringLength(100)]
    public string Kundenname { get; set; } = null!;

    public bool Intern { get; set; }

    public bool Aktiv { get; set; }

    [StringLength(1000)]
    public string? Adresse { get; set; }

    [InverseProperty("Kunde")]
    public virtual ICollection<TProjekt> TProjekt { get; set; } = new List<TProjekt>();

    [ForeignKey("TeamId")]
    [InverseProperty("TKunde")]
    public virtual TTeam? Team { get; set; }
}
