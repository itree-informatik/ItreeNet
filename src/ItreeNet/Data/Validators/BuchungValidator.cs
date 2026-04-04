using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class BuchungValidator : AbstractValidator<Buchung>
    {
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
                    var minutes = date!.Value.Minute;

                    if (minutes != 0 && minutes != 15 && minutes != 30 && minutes != 45)
                    {
                        context.AddFailure("Bitte nur viertelstündig raportieren.");
                    }
                });
            });

            When(b => b.ZeitBis != null, () =>
            {
                RuleFor(x => x.ZeitBis).Custom((date, context) =>
                {
                    var minutes = date!.Value.Minute;

                    if (minutes != 0 && minutes != 15 && minutes != 30 && minutes != 45)
                    {
                        context.AddFailure("Bitte nur viertelstündig raportieren.");
                    }
                });
            });

            When(b => b.Zeit != null, () =>
            {
                RuleFor(x => x.Zeit).Custom((zeit, context) =>
                {
                    if (zeit is null)
                        return;

                    // Prüft, ob Zeit ein Vielfaches von 0.25 ist (z.B. 0.25, 0.5, 0.75, 1.0, ...)
                    if ((zeit.Value * 100) % 25 != 0)
                    {
                        context.AddFailure("Bitte nur viertelstündig raportieren.");
                    }
                });
            });

            When(b => b.VorgangId == Guid.Parse("4F8ACC08-6C32-4C84-A3FB-2C17D1274AA1"), () =>
            {
                RuleFor(x => x.Zeit)
                    .GreaterThanOrEqualTo(4).WithMessage("Bitte mindestens 4h als Ferien eingeben");
            });
        }
    }
}
