using FluentValidation;
using MediatR;

namespace LibraryManagementSystem.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                
                // Thực hiện validation tuần tự thay vì đồng thời để tránh các vấn đề với DbContext
                var failures = new List<FluentValidation.Results.ValidationFailure>();
                
                foreach(var validator in _validators)
                {
                    var result = await validator.ValidateAsync(context, cancellationToken);
                    if (!result.IsValid)
                    {
                        failures.AddRange(result.Errors);
                    }
                }

                if (failures.Count != 0)
                {
                    throw new ValidationException(failures);
                }
            }

            return await next();
        }
    }
} 