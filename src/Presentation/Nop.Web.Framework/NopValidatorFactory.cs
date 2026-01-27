using System;
using FluentValidation;
using Nop.Core.Infrastructure;

namespace Nop.Web.Framework.Validators
{
    public class NopValidatorFactory : IValidatorFactory
    {
        public IValidator<T> GetValidator<T>()
        {
            return EngineContext.Current.Resolve<IValidator<T>>();
        }

        public IValidator GetValidator(Type type)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(type);
            return EngineContext.Current.Resolve(validatorType) as IValidator;
        }
    }
}
