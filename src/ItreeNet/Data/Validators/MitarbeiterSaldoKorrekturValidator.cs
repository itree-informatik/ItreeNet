using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class MitarbeiterSaldoKorrekturValidator : AbstractValidator<MitarbeiterSaldoKorrektur>
    {
        public MitarbeiterSaldoKorrekturValidator()
        {
            RuleFor(m => m.MitarbeiterId).NotEmpty().WithMessage("Bitte einen Mitarbeiter auswählen.");
            RuleFor(m => m.Jahr).GreaterThan(2000).WithMessage("Das Jahr muss grösser als 2000 sein");
            RuleFor(m => m.Monat).GreaterThan(0).WithMessage("Der Monat muss mind. 1 sein")
                .LessThan(12).WithMessage("Der Monat darf nicht grösser als 12 sein");
            RuleFor(m => m.Grund).NotNull().WithMessage("Bitte einen Grund eintragen");

            // Ferien oder Stunden müssen grösser als 0 sein
            RuleFor(m => m.Ferien).Custom((ferien, context) =>
            {
                var obj = context.InstanceToValidate;

                if (ferien == decimal.Zero && obj.Stunden == decimal.Zero)
                {
                    context.AddFailure("Ferien oder Stunden bitte ausfüllen");
                }
            });

            RuleFor(m => m.Stunden).Custom((stunden, context) =>
            {
                var obj = context.InstanceToValidate;

                if (stunden == decimal.Zero && obj.Ferien == decimal.Zero)
                {
                    context.AddFailure("Ferien oder Stunden bitte ausfüllen");
                }
            });
        }
    }
}
