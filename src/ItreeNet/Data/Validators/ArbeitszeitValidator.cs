using FluentValidation;
using ItreeNet.Data.Models;

namespace ItreeNet.Data.Validators
{
    public class ArbeitszeitValidator : AbstractValidator<Arbeitszeit>
    {
        public ArbeitszeitValidator()
        {
            RuleFor(b => b.Jahr).GreaterThan(DateTime.Now.Year - 1).WithMessage($"Jahr muss höher als {DateTime.Now.Year - 1} sein")
                                          .LessThan(2100).WithMessage("Jahr muss niedriger als 2100 sein")
                                          .NotEmpty().WithMessage("Bitte ein Jahr eintragen.");
            RuleFor(b => b.Monat).NotEmpty().WithMessage("Bitte einen Monat eintragen.")
                                          .GreaterThan(0).WithMessage("Monat muss grösser als 0 sein")
                                          .LessThanOrEqualTo(12).WithMessage("Monat darf nicht grösser als 12 sein");
            RuleFor(b => b.Zeit).NotEmpty().WithMessage("Bitte eine Arbeitszeit angeben.");
            RuleFor(b => b.Tagesarbeitszeit).NotEmpty().WithMessage("Bitte eine AzureId eintragen.");
        }
    }
}
