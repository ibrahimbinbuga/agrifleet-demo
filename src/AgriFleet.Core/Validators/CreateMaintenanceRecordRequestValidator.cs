using AgriFleet.Core.Dtos;
using FluentValidation;

namespace AgriFleet.Core.Validators;

public class CreateMaintenanceRecordRequestValidator : AbstractValidator<CreateMaintenanceRecordRequest>
{
    public CreateMaintenanceRecordRequestValidator()
    {
        RuleFor(x => x.Type).IsInEnum().WithMessage("Geçersiz bakım tipi.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama zorunludur.")
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir.");
        RuleFor(x => x.PerformedBy).NotEmpty().WithMessage("Bakımı yapan kişi zorunludur.")
            .MaximumLength(128).WithMessage("Bakımı yapan kişi en fazla 128 karakter olabilir.");
    }
}
