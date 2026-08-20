using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class FerienArbeitspensumValidator : AbstractValidator<FerienArbeitspensum>
    {
        public FerienArbeitspensumValidator()
        {
            RuleFor(f => f.MitarbeiterId).NotEmpty().WithMessage("Bitte einen Mitarbeiter auswählen.");
            RuleFor(f => f.GueltigAb).NotNull().WithMessage("Bitte das Gültig ab definieren.");
            RuleFor(f => f.FerienProJahr).GreaterThan(0).WithMessage("Bitte Ferien eintragen.");
            RuleFor(f => f.Arbeitspensum).GreaterThan(0).WithMessage("Bitte das Arbeitspensum eintragen");

            // Ohne Wochentag ist kein Tagespensum berechenbar (Division durch 0 im PensumService)
            RuleFor(f => f.Montag).Custom((_, context) =>
            {
                var obj = context.InstanceToValidate;

                if (!obj.Montag && !obj.Dienstag && !obj.Mittwoch && !obj.Donnerstag && !obj.Freitag)
                {
                    context.AddFailure("Bitte mindestens einen Wochentag auswählen");
                }
            });
        }
    }
}
