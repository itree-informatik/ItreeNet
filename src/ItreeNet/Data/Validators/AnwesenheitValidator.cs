using FluentValidation;
using ItreeNet.Data.Enums;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class AnwesenheitValidator : AbstractValidator<Anwesenheit>
    {
        private static readonly string[] GueltigeTypen = [EnumAnwesenheitTyp.Anwesenheit, EnumAnwesenheitTyp.Ferien, EnumAnwesenheitTyp.Gleitzeit, EnumAnwesenheitTyp.Krank, EnumAnwesenheitTyp.Abwesenheit];

        public AnwesenheitValidator()
        {
            RuleFor(a => a.Datum).NotNull().WithMessage("Bitte ein Datum eintragen.");
            RuleFor(a => a.MitarbeiterId).NotEmpty().WithMessage("MitarbeiterId fehlt.");

            RuleFor(a => a.Typ)
                .NotEmpty().WithMessage("Bitte einen Typ auswählen.")
                .Must(t => GueltigeTypen.Contains(t)).WithMessage("Ungültiger Typ.");

            RuleFor(a => a.Zeit)
                .NotNull().WithMessage("Bitte eine Zeit eintragen.")
                .GreaterThan(0).WithMessage("Zeit muss grösser als 0 sein.");

            When(a => a.Zeit != null, () =>
            {
                RuleFor(x => x.Zeit).Custom((zeit, context) =>
                {
                    if (zeit is null) return;

                    if ((zeit.Value * 100) % 25 != 0)
                    {
                        context.AddFailure("Bitte nur viertelstündig erfassen.");
                    }
                });
            });

            When(a => a.ZeitVon != null, () =>
            {
                RuleFor(x => x.ZeitVon).Custom((date, context) =>
                {
                    var minutes = date!.Value.Minute;
                    if (minutes != 0 && minutes != 15 && minutes != 30 && minutes != 45)
                    {
                        context.AddFailure("Bitte nur viertelstündig erfassen.");
                    }
                });
            });

            When(a => a.ZeitBis != null, () =>
            {
                RuleFor(x => x.ZeitBis).Custom((date, context) =>
                {
                    var minutes = date!.Value.Minute;
                    if (minutes != 0 && minutes != 15 && minutes != 30 && minutes != 45)
                    {
                        context.AddFailure("Bitte nur viertelstündig erfassen.");
                    }
                });
            });

            RuleFor(a => a.Notiz).MaximumLength(500).WithMessage("Notiz darf nicht länger als 500 Zeichen sein.");

            When(a => a.Typ == EnumAnwesenheitTyp.Abwesenheit, () =>
            {
                RuleFor(x => x.Notiz).NotEmpty().WithMessage("Bitte eine Notiz für Abwesenheit erfassen.");
            });

            When(a => a.Typ == EnumAnwesenheitTyp.Ferien, () =>
            {
                RuleFor(x => x.Zeit)
                    .GreaterThanOrEqualTo(4).WithMessage("Bitte mindestens 4h als Ferien eingeben.");
            });
        }
    }
}
