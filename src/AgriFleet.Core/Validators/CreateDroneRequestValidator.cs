using AgriFleet.Core.Dtos;
using FluentValidation;

namespace AgriFleet.Core.Validators;

public class CreateDroneRequestValidator : AbstractValidator<CreateDroneRequest>
{
    private static readonly string[] SupportedModels = ["AG-10", "AG-16"];

    public CreateDroneRequestValidator()
    {
        RuleFor(x => x.SerialNumber).NotEmpty().WithMessage("Seri numarası zorunludur.")
            .MaximumLength(64).WithMessage("Seri numarası en fazla 64 karakter olabilir.");
        RuleFor(x => x.Model).Must(model => SupportedModels.Contains(model))
            .WithMessage("Model AG-10 veya AG-16 olmalıdır.");
        RuleFor(x => x.TankCapacityLiters).InclusiveBetween(1, 100)
            .WithMessage("Tank kapasitesi 1 ile 100 litre arasında olmalıdır.");
    }
}
