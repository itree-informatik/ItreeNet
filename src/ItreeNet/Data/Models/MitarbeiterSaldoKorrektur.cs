using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Data.Models;
public class MitarbeiterSaldoKorrektur
{
    public Guid Id { get; set; }
    public Guid MitarbeiterId { get; set; }
    public int Jahr { get; set; }
    public int Monat { get; set; }
    public decimal? Stunden { get; set; }
    public int? Ferien { get; set; }
    public string? Grund { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }

    public Mitarbeiter? CreatedByNavigation { get; set; }
    public Mitarbeiter? Mitarbeiter { get; set; }
}
