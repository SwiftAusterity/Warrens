using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural.PropertyValidation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class IntValueRangeValidator : ValidationAttribute
    {
        public bool Optional { get; set; }

        public IntValueRangeValidator() : base()
        {
            Optional = false;
        }

        public override bool IsValid(object value)
        {
            ValueRange<int> item = value as ValueRange<int>;

            if (item == null)
                return !Optional;

            return item.Low <= item.High;
        }
    }
}