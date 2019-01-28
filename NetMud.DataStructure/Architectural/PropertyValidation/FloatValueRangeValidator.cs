using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural.PropertyValidation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class FloatValueRangeValidator : ValidationAttribute
    {
        public bool Optional { get; set; }

        public FloatValueRangeValidator() : base()
        {
            Optional = false;
        }

        public override bool IsValid(object value)
        {
            ValueRange<float> item = value as ValueRange<float>;

            if (item == null)
            {
                return !Optional;
            }

            return item.Low <= item.High;
        }
    }
}