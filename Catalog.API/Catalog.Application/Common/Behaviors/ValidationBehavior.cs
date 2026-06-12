using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Application.Common.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TReponse>
        : IPipelineBehavior<TRequest, TReponse>
         where TRequest : notnull
    {
        private readonly  IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;
        public async Task<TReponse> Handle(TRequest request, RequestHandlerDelegate<TReponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any()) return await next();
            var _context = new ValidationContext<TRequest>(request);

            var errors = _validators.Select(v=> v.Validate(_context))
                             .SelectMany(r=> r.Errors)
                             .Where(f=> f !=null)
                             .ToList();
            if (errors.Count != 0)  throw new ValidationException(errors);

            return await next();


        }
    }
}
