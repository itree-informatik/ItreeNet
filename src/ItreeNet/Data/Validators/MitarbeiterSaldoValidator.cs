using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class MitarbeiterSaldoValidator : AbstractValidator<MitarbeiterSaldo>
    {
        public MitarbeiterSaldoValidator()
        {
            RuleFor(m => m.MitarbeiterId).NotEmpty().WithMessage("Bitte einen Mitarbeiter auswählen.");
            RuleFor(m => m.Jahr).GreaterThan(2000).WithMessage("Das Jahr muss grösser als 2000 sein");
            RuleFor(m => m.Monat).GreaterThan(0).WithMessage("Monat muss grösser als 0 sein")
                .LessThanOrEqualTo(12).WithMessage("Der Monat darf nicht grösser als 12 sein");
        }
    }
}
