using Catalog.Application.Features.Resources.Commands.CreateResource;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Features.Resources.Commands.UpdateResource
{
  
    public sealed class UpdateResourceValidator : AbstractValidator<UpdateResourceCommand>
    {
        public UpdateResourceValidator() {
            RuleFor(x => x.id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Capacity).GreaterThanOrEqualTo(100);
            RuleFor(x => x.Building).NotEmpty();
        }
    }
}
