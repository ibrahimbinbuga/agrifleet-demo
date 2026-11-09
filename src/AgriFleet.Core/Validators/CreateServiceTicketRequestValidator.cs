using AgriFleet.Core.Dtos;
using FluentValidation;

namespace AgriFleet.Core.Validators;

public class CreateServiceTicketRequestValidator : AbstractValidator<CreateServiceTicketRequest>
{
    public CreateServiceTicketRequestValidator()
    {
        RuleFor(x => x.DroneId).NotEmpty().WithMessage("İHA seçilmelidir.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlık zorunludur.")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama zorunludur.")
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");
        RuleFor(x => x.Priority).IsInEnum().WithMessage("Geçersiz öncelik.");
    }
}
