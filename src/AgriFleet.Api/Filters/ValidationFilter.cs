using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgriFleet.Api.Filters;

public class ValidationFilter(IServiceProvider services) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values.OfType<object>())
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            if (services.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument),
                context.HttpContext.RequestAborted);

            if (result.IsValid)
                continue;

            var problem = new ValidationProblemDetails(result.ToDictionary())
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Gönderilen veriler geçersiz.",
            };

            context.Result = new BadRequestObjectResult(problem);
            return;
        }

        await next();
    }
}
