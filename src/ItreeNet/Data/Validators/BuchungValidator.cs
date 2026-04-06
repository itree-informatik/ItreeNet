using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class BuchungValidator : AbstractValidator<Buchung>
    {
        /// <summary>
        /// Buchungsintervall in Minuten (z.B. 15 = viertelstündig, 1 = minütig).
        /// Wird vom UI gesetzt wenn das Projekt bekannt ist.
        /// </summary>
        public int? Intervall { get; set; } = 15;

        public BuchungValidator()
        {
            RuleFor(b => b.Datum).NotNull().WithMessage("Bitte ein Datum eintragen.");
            RuleFor(b => b.Zeit).NotNull().WithMessage("Bitte eine Zeit eintragen.").GreaterThan(0).WithMessage("Zeit muss grösser als 0 sein.");
            RuleFor(b => b.VorgangId).NotNull().WithMessage("Bitte einen Vorgang eintragen.").NotEmpty().WithMessage("Bitte einen Vorgang eintragen.");
            RuleFor(b => b.MitarbeiterId).NotNull().WithMessage("MitarbeiterId fehlt.");
            RuleFor(b => b.Buchungstext).NotNull().MaximumLength(100).WithMessage("Text darf nicht länger als 100 Zeichen sein");

            When(b => b.DatumBis != null, () =>
            {
                RuleFor(b => b.DatumBis)
                    .GreaterThanOrEqualTo(b => b.Datum)
                    .WithMessage("Datum bis muss nach Datum von sein.");
            });

            When(b => b.ZeitVon != null, () =>
            {
                RuleFor(x => x.ZeitVon).Custom((date, context) =>
                {
                    if (date is null || Intervall is null or 1) return;
                    var minutes = date.Value.Minute;
                    if (minutes % Intervall.Value != 0)
                    {
                        context.AddFailure($"Bitte nur im {Intervall}-Minuten-Takt raportieren.");
                    }
                });
            });

            When(b => b.ZeitBis != null, () =>
            {
                RuleFor(x => x.ZeitBis).Custom((date, context) =>
                {
                    if (date is null || Intervall is null or 1) return;
                    var minutes = date.Value.Minute;
                    if (minutes % Intervall.Value != 0)
                    {
                        context.AddFailure($"Bitte nur im {Intervall}-Minuten-Takt raportieren.");
                    }
                });
            });

            When(b => b.Zeit != null, () =>
            {
                RuleFor(x => x.Zeit).Custom((zeit, context) =>
                {
                    if (zeit is null || Intervall is null or 1) return;
                    if (zeit.Value % Intervall.Value != 0)
                    {
                        context.AddFailure($"Bitte nur im {Intervall}-Minuten-Takt raportieren.");
                    }
                });
            });

            When(b => b.VorgangId == Guid.Parse("4F8ACC08-6C32-4C84-A3FB-2C17D1274AA1"), () =>
            {
                RuleFor(x => x.Zeit)
                    .GreaterThanOrEqualTo(240).WithMessage("Bitte mindestens 4h (240min) als Ferien eingeben");
            });
        }
    }
}
