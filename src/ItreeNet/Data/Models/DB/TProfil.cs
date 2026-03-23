using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItreeNet.Data.Models.DB;

[Table("T_Profil")]
public partial class TProfil
{
    [Key]
    public Guid Id { get; set; }

    public Guid MitarbeiterId { get; set; }

    [StringLength(4000)]
    public string Wert { get; set; } = null!;

    [ForeignKey("MitarbeiterId")]
    [InverseProperty("TProfil")]
    public virtual TMitarbeiter Mitarbeiter { get; set; } = null!;
}
