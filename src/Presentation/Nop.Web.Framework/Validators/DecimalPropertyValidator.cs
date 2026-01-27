using System;
using FluentValidation;
using FluentValidation.Validators;
using Nop.Services.Catalog;

namespace Nop.Web.Framework.Validators
{
    /// <summary>
    /// Decimal property validator - simplified for FluentValidation 11+
    /// </summary>
    public class DecimalPropertyValidator<T, TProperty> : PropertyValidator<T, TProperty>
    {
        private readonly decimal _maxValue;

        public DecimalPropertyValidator(decimal maxValue)
        {
            _maxValue = maxValue;
        }

        public override string Name => "DecimalPropertyValidator";

        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            if (value == null) return true;
            if (decimal.TryParse(value.ToString(), out var decimalValue))
            {
                return RoundingHelper.RoundPrice(decimalValue) < _maxValue;
            }
            return false;
        }
    }
}
