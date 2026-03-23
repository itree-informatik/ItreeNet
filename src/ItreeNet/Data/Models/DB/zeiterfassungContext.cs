using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Data.Models.DB;

public partial class ZeiterfassungContext : DbContext
{
    public ZeiterfassungContext()
    {
    }

    public ZeiterfassungContext(DbContextOptions<ZeiterfassungContext> options)
        : base(options)
    {
    }
    
    public virtual DbSet<TAnwesenheit> TAnwesenheit { get; set; }

    public virtual DbSet<TArbeitszeit> TArbeitszeit { get; set; }

    public virtual DbSet<TArbeitszeitReduktion> TArbeitszeitReduktion { get; set; }

    public virtual DbSet<TBuchung> TBuchung { get; set; }

    public virtual DbSet<TFerienArbeitspensum> TFerienArbeitspensum { get; set; }

    public virtual DbSet<TFrontendtest> TFrontendtest { get; set; }

    public virtual DbSet<TFrontendtestBild> TFrontendtestBild { get; set; }

    public virtual DbSet<TFrontendtestDetail> TFrontendtestDetail { get; set; }

    public virtual DbSet<TKunde> TKunde { get; set; }

    public virtual DbSet<TLog> TLog { get; set; }

    public virtual DbSet<TMitarbeiter> TMitarbeiter { get; set; }

    public virtual DbSet<TMitarbeiterSaldo> TMitarbeiterSaldo { get; set; }

    public virtual DbSet<TMitarbeiterSaldoKorrektur> TMitarbeiterSaldoKorrektur { get; set; }

    public virtual DbSet<TMitarbeiterTeam> TMitarbeiterTeam { get; set; }

    public virtual DbSet<TPipelineRuns> TPipelineRuns { get; set; }

    public virtual DbSet<TProfil> TProfil { get; set; }

    public virtual DbSet<TProjekt> TProjekt { get; set; }

    public virtual DbSet<TSpesen> TSpesen { get; set; }

    public virtual DbSet<TTeam> TTeam { get; set; }

    public virtual DbSet<TVorgang> TVorgang { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:App");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Default Schema setzen
        modelBuilder.HasDefaultSchema("dbo");

        // Alles lowercase für PostgreSQL
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetSchema(entity.GetSchema()?.ToLower());
            entity.SetTableName(entity.GetTableName()?.ToLower());

            foreach (var property in entity.GetProperties())
                property.SetColumnName(property.GetColumnName()?.ToLower());

            foreach (var key in entity.GetKeys())
                key.SetName(key.GetName()?.ToLower());

            foreach (var key in entity.GetForeignKeys())
                key.SetConstraintName(key.GetConstraintName()?.ToLower());

            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(index.GetDatabaseName()?.ToLower());
        }
        base.OnModelCreating(modelBuilder);



        modelBuilder.Entity<TAnwesenheit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Anwesenheit");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Mitarbeiter).WithMany(p => p.TAnwesenheit)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Anwesenheit_Mitarbeiter");
        });

        modelBuilder.Entity<TArbeitszeit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Arbeitszeit");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Tagesarbeitszeit).HasDefaultValue(8m);
        });

        modelBuilder.Entity<TArbeitszeitReduktion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ArbeitszeitReduktion");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<TBuchung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Buchung_1");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.TBuchungChangedByNavigation).HasConstraintName("FK_Buchung_ChangedBy");

            entity.HasOne(d => d.Mitarbeiter).WithMany(p => p.TBuchungMitarbeiter)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Buchung_Mitarbeiter");

            entity.HasOne(d => d.OriginalVorgang).WithMany(p => p.TBuchungOriginalVorgang).HasConstraintName("FK_Buchung_OriginalVorgang");

            entity.HasOne(d => d.Vorgang).WithMany(p => p.TBuchungVorgang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Buchung_Vorgang");
        });

        modelBuilder.Entity<TFerienArbeitspensum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_FerienArbeitspensum");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Arbeitspensum).HasDefaultValue(100m);
            entity.Property(e => e.Dienstag).HasDefaultValue(true);
            entity.Property(e => e.Donnerstag).HasDefaultValue(true);
            entity.Property(e => e.FerienProJahr).HasDefaultValue(25m);
            entity.Property(e => e.Freitag).HasDefaultValue(true);
            entity.Property(e => e.Mittwoch).HasDefaultValue(true);
            entity.Property(e => e.Montag).HasDefaultValue(true);

            entity.HasOne(d => d.Mitarbeiter).WithMany(p => p.TFerienArbeitspensum)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FerienArbeitspensum_Mitarbeiter");
        });

        modelBuilder.Entity<TFrontendtest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Frontendtest");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<TFrontendtestBild>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_FrontendtestBild");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Parent).WithMany(p => p.TFrontendtestBild).HasConstraintName("FK_T_FrontendtestBild_T_FrontendtestDetail");
        });

        modelBuilder.Entity<TFrontendtestDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_FrontendtestDetail");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Parent).WithMany(p => p.TFrontendtestDetail).HasConstraintName("FK_T_FrontendtestDetail_T_Frontendtest");
        });

        modelBuilder.Entity<TKunde>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Kunde");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Aktiv).HasDefaultValue(true);

            entity.HasOne(d => d.Team).WithMany(p => p.TKunde).HasConstraintName("FK_T_Kunde_T_Team");
        });

        modelBuilder.Entity<TMitarbeiter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Mitarbeiter_1");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Aktiv).HasDefaultValue(true);
            entity.Property(e => e.Intern).HasDefaultValue(true);
        });

        modelBuilder.Entity<TMitarbeiterSaldo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_MitarbeiterSaldo_1");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<TMitarbeiterSaldoKorrektur>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TMitarbeiterSaldoKorrekturCreatedByNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_T_MitarbeiterSaldoKorrektur_T_Mitarbeiter1");

            entity.HasOne(d => d.Mitarbeiter).WithMany(p => p.TMitarbeiterSaldoKorrekturMitarbeiter)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_T_MitarbeiterSaldoKorrektur_T_Mitarbeiter");
        });

        modelBuilder.Entity<TMitarbeiterTeam>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Mitarbeiter).WithMany(p => p.TMitarbeiterTeam)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_T_MitarbeiterTeam_T_Mitarbeiter");

            entity.HasOne(d => d.Team).WithMany(p => p.TMitarbeiterTeam)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_T_MitarbeiterTeam_T_Team");
        });

        modelBuilder.Entity<TPipelineRuns>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_PipelineRuns");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<TProfil>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Mitarbeiter).WithMany(p => p.TProfil)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Profil_Mitarbeiter");
        });

        modelBuilder.Entity<TProjekt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Projekt_1");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Aktiv).HasDefaultValue(true);

            entity.HasOne(d => d.Kunde).WithMany(p => p.TProjekt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Projekt_Kunde");
        });

        modelBuilder.Entity<TSpesen>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Spesen");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Mitarbeiter).WithMany(p => p.TSpesen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Spesen_Mitarbeiter");
        });

        modelBuilder.Entity<TTeam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Team");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Aktiv).HasDefaultValue(true);
        });

        modelBuilder.Entity<TVorgang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Vorgang_1");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Aktiv).HasDefaultValue(true);

            entity.HasOne(d => d.Projekt).WithMany(p => p.TVorgang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vorgang_Projekt");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
