using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Commands.CreateResource;

public sealed class CreateResourceValidator : AbstractValidator<CreateResourceCommand>

{
    public CreateResourceValidator() {
        RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor( x=> x.Type)
               .IsInEnum().WithMessage("Invalid resource type.");

        RuleFor(x=> x.Capacity)
             .GreaterThan(0).WithMessage("Capacity must be >= 0.");

        RuleFor(x => x.Building)
              .NotEmpty().WithMessage("Building is required.");


    }

}
